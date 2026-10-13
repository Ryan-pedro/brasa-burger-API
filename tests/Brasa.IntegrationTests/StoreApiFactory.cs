using Brasa.Domain.Identity;
using Brasa.Domain.StoreSettings;
using Brasa.Infrastructure.Identity;
using Brasa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
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

    public const string Owner = "dono@teste.dev";
    public const string Manager = "gerente@teste.dev";
    public const string Kitchen = "cozinha@teste.dev";
    public const string Disabled = "desativado@teste.dev";

    /// <summary>Usuário reservado para o teste de bloqueio, para não atrapalhar os outros.</summary>
    public const string UserToLockOut = "bloqueio@teste.dev";

    /// <summary>Usuário reservado para o teste de desativação com sessão aberta.</summary>
    public const string UserToDisable = "sera-desativado@teste.dev";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" evita a semeadura automática do ambiente de desenvolvimento.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", TestConnectionString);
    }

    public async Task InitializeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        db.Stores.Add(new Store(StoreName, "America/Sao_Paulo"));
        await db.SaveChangesAsync();

        await CreateUserAsync(users, Owner, StaffRole.Owner);
        await CreateUserAsync(users, Manager, StaffRole.Manager);
        await CreateUserAsync(users, Kitchen, StaffRole.Kitchen);
        await CreateUserAsync(users, Disabled, StaffRole.Manager, isActive: false);
        await CreateUserAsync(users, UserToLockOut, StaffRole.Manager);
        await CreateUserAsync(users, UserToDisable, StaffRole.Manager);
    }

    Task IAsyncLifetime.DisposeAsync() => base.DisposeAsync().AsTask();

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
