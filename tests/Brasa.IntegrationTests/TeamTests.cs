using System.Net;
using System.Net.Http.Json;
using Brasa.Application.Identity.Session;
using Brasa.Application.Identity.Team;
using Brasa.Domain.Identity;
using Brasa.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class TeamTests(StoreApiFactory factory)
{
    private const string NewPassword = "senha-nova-da-equipe";

    [Fact]
    public async Task Dono_convida_e_a_pessoa_entra_pelo_link_com_o_papel_do_convite()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var email = UniqueEmail();

        var invitation = await owner.InviteAsync(email, "Kitchen");
        var visitor = factory.NewClient();

        var preview = await (await visitor.GetAsync($"/api/v1/admin/invitations/{invitation.Token}")).ReadAsync<InvitationPreviewResponse>();
        Assert.Equal(email, preview.Email);
        Assert.Equal("Kitchen", preview.Role);
        Assert.Equal(StoreApiFactory.StoreName, preview.StoreName);

        var session = await (await visitor.PostAsJsonAsync($"/api/v1/admin/invitations/{invitation.Token}/accept", new { password = NewPassword })).ReadAsync<SessionResponse>();
        Assert.Equal("Kitchen", session.Role);
        Assert.Equal(HttpStatusCode.OK, (await visitor.GetAsync("/api/v1/admin/session")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await factory.NewClient().LoginAsync(email, NewPassword)).StatusCode);

        var team = await (await owner.GetAsync("/api/v1/admin/team")).ReadAsync<TeamResponse>();
        Assert.Contains(team.Members, member => member.Email == email && member.Role == "Kitchen" && member.IsActive);
        Assert.DoesNotContain(team.Invitations, pending => pending.Id == invitation.Id);
    }

    [Fact]
    public async Task Convite_aceito_revogado_ou_expirado_nao_abre_mais()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var visitor = factory.NewClient();

        var accepted = await owner.InviteAsync(UniqueEmail(), "Manager");
        await visitor.PostAsJsonAsync($"/api/v1/admin/invitations/{accepted.Token}/accept", new { password = NewPassword });
        var reused = await factory.NewClient().PostAsJsonAsync($"/api/v1/admin/invitations/{accepted.Token}/accept", new { password = NewPassword });
        Assert.Equal(HttpStatusCode.NotFound, reused.StatusCode);
        Assert.Equal("invitation_not_found", await reused.ReadErrorCodeAsync());

        var revoked = await owner.InviteAsync(UniqueEmail(), "Manager");
        (await owner.PostAsync($"/api/v1/admin/team/invitations/{revoked.Id}/revoke", null)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync($"/api/v1/admin/invitations/{revoked.Token}")).StatusCode);

        var expired = await owner.InviteAsync(UniqueEmail(), "Manager");
        factory.Clock.Advance(StaffInvitation.Validity);
        Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync($"/api/v1/admin/invitations/{expired.Token}")).StatusCode);
    }

    [Fact]
    public async Task Novo_convite_para_o_mesmo_email_substitui_o_anterior()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var email = UniqueEmail();

        var first = await owner.InviteAsync(email, "Manager");
        var second = await owner.InviteAsync(email, "Kitchen");

        Assert.Equal(HttpStatusCode.NotFound, (await factory.NewClient().GetAsync($"/api/v1/admin/invitations/{first.Token}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await factory.NewClient().GetAsync($"/api/v1/admin/invitations/{second.Token}")).StatusCode);
    }

    [Fact]
    public async Task Senha_fraca_no_aceite_responde_400_e_o_convite_continua_valendo()
    {
        var invitation = await (await factory.SignedInAsync(StoreApiFactory.Owner)).InviteAsync(UniqueEmail(), "Manager");
        var visitor = factory.NewClient();

        var weak = await visitor.PostAsJsonAsync($"/api/v1/admin/invitations/{invitation.Token}/accept", new { password = "curta" });

        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);
        Assert.Contains("password", await weak.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await visitor.PostAsJsonAsync($"/api/v1/admin/invitations/{invitation.Token}/accept", new { password = NewPassword })).StatusCode);
    }

    [Fact]
    public async Task Convite_valida_campos_papel_e_email_ja_cadastrado()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);

        var invalid = await owner.PostAsJsonAsync("/api/v1/admin/team/invitations", new { email = "não-é-email", displayName = "", role = "Owner" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var body = await invalid.Content.ReadAsStringAsync();
        Assert.Contains("\"email\"", body);
        Assert.Contains("\"displayName\"", body);
        Assert.Contains("\"role\"", body);

        var existing = await owner.PostAsJsonAsync("/api/v1/admin/team/invitations", new { email = StoreApiFactory.Manager, displayName = "Gerente", role = "Manager" });
        Assert.Equal(HttpStatusCode.Conflict, existing.StatusCode);
        Assert.Equal("team_member_exists", await existing.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task So_quem_gerencia_a_equipe_acessa_as_rotas_de_equipe()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.NewClient().GetAsync("/api/v1/admin/team")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await (await factory.SignedInAsync(StoreApiFactory.Manager)).GetAsync("/api/v1/admin/team")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await (await factory.SignedInAsync(StoreApiFactory.Kitchen)).GetAsync("/api/v1/admin/team")).StatusCode);

        var withoutCsrf = await factory.SignedInAsync(StoreApiFactory.Owner);
        withoutCsrf.DefaultRequestHeaders.Remove(TestApi.CsrfHeader);
        var response = await withoutCsrf.PostAsJsonAsync("/api/v1/admin/team/invitations", new { email = UniqueEmail(), displayName = "X", role = "Kitchen" });
        Assert.Equal("csrf_validation_failed", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Dono_muda_o_papel_e_desativa_uma_pessoa_que_perde_a_sessao()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var email = UniqueEmail();
        var invitation = await owner.InviteAsync(email, "Manager");
        var member = factory.NewClient();
        var session = await (await member.PostAsJsonAsync($"/api/v1/admin/invitations/{invitation.Token}/accept", new { password = NewPassword })).ReadAsync<SessionResponse>();

        var changed = await (await owner.PutAsJsonAsync($"/api/v1/admin/team/members/{session.User.Id}/role", new { role = "Kitchen" })).ReadAsync<TeamMemberResponse>();
        Assert.Equal("Kitchen", changed.Role);
        Assert.Equal("Kitchen", (await (await member.GetAsync("/api/v1/admin/session")).ReadAsync<SessionResponse>()).Role);

        (await owner.PutAsJsonAsync($"/api/v1/admin/team/members/{session.User.Id}/status", new { isActive = false })).EnsureSuccessStatusCode();
        var dropped = await member.GetAsync("/api/v1/admin/session");
        Assert.Equal(HttpStatusCode.Unauthorized, dropped.StatusCode);

        (await owner.PutAsJsonAsync($"/api/v1/admin/team/members/{session.User.Id}/status", new { isActive = true })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, (await factory.NewClient().LoginAsync(email, NewPassword)).StatusCode);
    }

    [Fact]
    public async Task Dono_nao_altera_a_propria_conta_nem_a_de_outro_proprietario()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var self = await (await owner.GetAsync("/api/v1/admin/session")).ReadAsync<SessionResponse>();

        var own = await owner.PutAsJsonAsync($"/api/v1/admin/team/members/{self.User.Id}/status", new { isActive = false });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, own.StatusCode);
        Assert.Equal("cannot_change_own_account", await own.ReadErrorCodeAsync());

        var otherOwnerId = await CreateOwnerAsync();
        var other = await owner.PutAsJsonAsync($"/api/v1/admin/team/members/{otherOwnerId}/role", new { role = "Kitchen" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, other.StatusCode);
        Assert.Equal("owner_account_protected", await other.ReadErrorCodeAsync());

        var missing = await owner.PutAsJsonAsync($"/api/v1/admin/team/members/{Guid.NewGuid()}/role", new { role = "Kitchen" });
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    private async Task<Guid> CreateOwnerAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var email = UniqueEmail();
        var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = "Sócio", Role = StaffRole.Owner };
        Assert.True((await users.CreateAsync(user, StoreApiFactory.Password)).Succeeded);
        return user.Id;
    }

    private static string UniqueEmail() => $"equipe-{Guid.NewGuid():N}@teste.dev";
}
