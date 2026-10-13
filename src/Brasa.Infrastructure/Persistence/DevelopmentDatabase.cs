using Brasa.Domain.Identity;
using Brasa.Domain.StoreSettings;
using Brasa.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.Infrastructure.Persistence;

/// <summary>
/// Prepara o banco do ambiente de desenvolvimento: aplica as migrations e cria a loja
/// e a equipe de demonstração. Pode rodar várias vezes: só cria o que ainda não existe.
/// Nunca é chamado fora de desenvolvimento: em produção as migrations são um passo do pipeline.
/// </summary>
public static class DevelopmentDatabase
{
    /// <summary>Senha de todos os usuários de demonstração. Vale somente em desenvolvimento.</summary>
    public const string DemoPassword = "brasa-dev-1234";

    public static async Task MigrateAndSeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        await db.Database.MigrateAsync(cancellationToken);

        if (!await db.Stores.AnyAsync(cancellationToken))
        {
            db.Stores.Add(new Store("Brasa Burger", "America/Sao_Paulo"));
            await db.SaveChangesAsync(cancellationToken);
        }

        await EnsureUserAsync(users, "dono@brasa.dev", "Dona da Brasa", StaffRole.Owner);
        await EnsureUserAsync(users, "gerente@brasa.dev", "Gerente da Brasa", StaffRole.Manager);
        await EnsureUserAsync(users, "cozinha@brasa.dev", "Cozinha da Brasa", StaffRole.Kitchen);
    }

    private static async Task EnsureUserAsync(UserManager<AppUser> users, string email, string displayName, StaffRole role)
    {
        if (await users.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = displayName, Role = role };

        var result = await users.CreateAsync(user, DemoPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Não foi possível criar o usuário de demonstração {email}: {errors}");
        }
    }
}
