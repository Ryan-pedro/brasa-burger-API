using System.Net;
using System.Net.Http.Json;
using Brasa.Application.Identity.Session;
using Brasa.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.IntegrationTests;

/// <summary>
/// Entrada e sessão no painel. A regra central: papel e situação do usuário são
/// relidos do banco a cada requisição, nunca confiados ao cookie.
/// </summary>
[Collection(ApiCollection.Name)]
public class AdminAuthTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Login_valido_abre_a_sessao_com_o_papel_do_usuario()
    {
        var response = await factory.NewClient().LoginAsync(StoreApiFactory.Owner);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(session);
        Assert.Equal(StoreApiFactory.Owner, session.User.Email);
        Assert.Equal("Owner", session.Role);
        Assert.Contains("team.manage", session.Permissions);
    }

    [Fact]
    public async Task Depois_do_login_a_sessao_e_reconhecida_pelo_cookie()
    {
        var client = factory.NewClient();
        await client.LoginAsync(StoreApiFactory.Manager);

        var response = await client.GetAsync("/api/v1/admin/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(session);
        Assert.Equal("Manager", session.Role);
        Assert.DoesNotContain("team.manage", session.Permissions);
    }

    [Fact]
    public async Task O_cookie_de_sessao_nao_fica_acessivel_ao_javascript()
    {
        var response = await factory.NewClient().LoginAsync(StoreApiFactory.Owner);

        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Sem_sessao_a_rota_protegida_responde_401()
    {
        var response = await factory.NewClient().GetAsync("/api/v1/admin/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Senha_errada_responde_401_sem_dizer_se_o_email_existe()
    {
        var wrongPassword = await factory.NewClient().LoginAsync(StoreApiFactory.Owner, "senha-errada-000");
        var unknownEmail = await factory.NewClient().LoginAsync("ninguem@teste.dev", "senha-errada-000");

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        Assert.Equal("invalid_credentials", await wrongPassword.ReadErrorCodeAsync());
        Assert.Equal("invalid_credentials", await unknownEmail.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Usuario_desativado_nao_entra()
    {
        var response = await factory.NewClient().LoginAsync(StoreApiFactory.Disabled);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("account_disabled", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Login_sem_o_cabecalho_anti_csrf_e_recusado()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/v1/admin/auth/login",
            new { email = StoreApiFactory.Owner, password = StoreApiFactory.Password });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("csrf_validation_failed", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Requisicao_vinda_de_outra_origem_e_recusada()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/auth/login");
        request.Headers.Add("Origin", "http://site-malicioso.localhost");
        request.Content = JsonContent.Create(new { email = StoreApiFactory.Owner, password = StoreApiFactory.Password });

        var response = await factory.NewClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("csrf_validation_failed", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Papel_de_cozinha_nao_recebe_permissoes_administrativas()
    {
        var response = await factory.NewClient().LoginAsync(StoreApiFactory.Kitchen);

        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(session);
        Assert.Equal("Kitchen", session.Role);
        Assert.All(session.Permissions, permission => Assert.StartsWith("kitchen.", permission));
    }

    [Fact]
    public async Task Logout_encerra_a_sessao()
    {
        var client = factory.NewClient();
        await client.LoginAsync(StoreApiFactory.Owner);

        var logout = await client.PostAsync("/api/v1/admin/auth/logout", content: null);
        var afterLogout = await client.GetAsync("/api/v1/admin/session");

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }

    [Fact]
    public async Task Usuario_desativado_perde_a_sessao_na_requisicao_seguinte()
    {
        var client = factory.NewClient();
        await client.LoginAsync(StoreApiFactory.UserToDisable);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/admin/session")).StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await users.FindByEmailAsync(StoreApiFactory.UserToDisable);
            Assert.NotNull(user);
            user.IsActive = false;
            await users.UpdateAsync(user);
        }

        var response = await client.GetAsync("/api/v1/admin/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("session_expired", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Cinco_senhas_erradas_bloqueiam_a_conta_mesmo_com_a_senha_certa_depois()
    {
        var client = factory.NewClient();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await client.LoginAsync(StoreApiFactory.UserToLockOut, "senha-errada-000");
        }

        var response = await client.LoginAsync(StoreApiFactory.UserToLockOut);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("account_locked", await response.ReadErrorCodeAsync());
    }
}
