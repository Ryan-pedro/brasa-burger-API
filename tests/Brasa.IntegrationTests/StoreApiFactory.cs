using Brasa.Application.Shared.Realtime;
using Brasa.Domain.Catalog;
using Brasa.Domain.Identity;
using Brasa.Domain.StoreSettings;
using Brasa.Infrastructure.Identity;
using Brasa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.IntegrationTests;

/// <summary>
/// Sobe a API em memória contra um PostgreSQL de verdade (o do Docker Compose),
/// em um banco separado, recriado a cada execução dos testes.
/// Requer: docker compose -f infra/compose/compose.dev.yml up -d
/// </summary>
public sealed class StoreApiFactory : WebApplicationFactory<Brasa.Api.AssemblyReference>, IAsyncLifetime
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=brasa_test;Username=brasa;Password=brasa_dev";

    public const string StoreName = "Loja de Teste";
    public const string Password = "senha-de-teste-123";
    public const string FakePixSecret = "segredo-do-pix-de-teste";

    public const string Owner = "dono@teste.dev";
    public const string Manager = "gerente@teste.dev";
    public const string Kitchen = "cozinha@teste.dev";
    public const string Disabled = "desativado@teste.dev";

    /// <summary>Usuário reservado para o teste de bloqueio, para não atrapalhar os outros.</summary>
    public const string UserToLockOut = "bloqueio@teste.dev";

    /// <summary>Usuário reservado para o teste de desativação com sessão aberta.</summary>
    public const string UserToDisable = "sera-desativado@teste.dev";

    /// <summary>Cardápio inicial dos testes: uma categoria visível e uma inativa.</summary>
    public const string VisibleCategory = "Lanches";
    public const string HiddenCategory = "Escondida";
    public const string AvailableProduct = "X-Teste";
    public const string UnavailableProduct = "Esgotado";
    public const string HiddenProduct = "Secreto";

    public TestClock Clock { get; } = new();

    public RecordingNotifier Notifier { get; } = new();

    public string ImagesPath { get; } = Path.Combine(Path.GetTempPath(), $"brasa-images-{Guid.NewGuid():N}");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" evita a semeadura automática do ambiente de desenvolvimento.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", TestConnectionString);
        builder.UseSetting("Jobs:Enabled", "false");
        builder.UseSetting("RateLimits:OrdersPerMinute", "100000");
        builder.UseSetting("RateLimits:QuotesPerMinute", "100000");
        builder.UseSetting("RateLimits:AgentCredentialsPerMinute", "100000");
        builder.UseSetting("Storage:Images:Path", ImagesPath);
        builder.UseSetting("Payments:Pix:Provider", "Fake");
        builder.UseSetting("Payments:Pix:FakeWebhookSecret", FakePixSecret);

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(Clock);
            services.AddSingleton<IRealtimeNotifier>(Notifier);
        });
    }

    public async Task InitializeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        db.Stores.Add(new Store(StoreName, "America/Sao_Paulo"));

        var visible = new Category(VisibleCategory, 0);
        var hidden = new Category(HiddenCategory, 1);
        hidden.Deactivate();
        db.Categories.AddRange(visible, hidden);

        var featured = new Product(visible.Id, AvailableProduct, "Pão, carne e queijo.", 19.90m, null, null, 0);
        featured.SetFeatured(true);
        var soldOut = new Product(visible.Id, UnavailableProduct, null, 12.50m, null, null, 1);
        soldOut.SetAvailability(false);
        db.Products.AddRange(featured, soldOut, new Product(hidden.Id, HiddenProduct, null, 5m, null, null, 0));

        await db.SaveChangesAsync();

        await CreateUserAsync(users, Owner, StaffRole.Owner);
        await CreateUserAsync(users, Manager, StaffRole.Manager);
        await CreateUserAsync(users, Kitchen, StaffRole.Kitchen);
        await CreateUserAsync(users, Disabled, StaffRole.Manager, isActive: false);
        await CreateUserAsync(users, UserToLockOut, StaffRole.Manager);
        await CreateUserAsync(users, UserToDisable, StaffRole.Manager);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();

        if (Directory.Exists(ImagesPath))
        {
            Directory.Delete(ImagesPath, recursive: true);
        }
    }

    private static async Task CreateUserAsync(UserManager<AppUser> users, string email, StaffRole role, bool isActive = true)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = email,
            Role = role,
            IsActive = isActive,
        };

        var result = await users.CreateAsync(user, Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Falha ao criar {email}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
