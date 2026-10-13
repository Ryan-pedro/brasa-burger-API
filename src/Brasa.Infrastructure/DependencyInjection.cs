using Brasa.Application.Identity;
using Brasa.Application.Shared.Persistence;
using Brasa.Infrastructure.Identity;
using Brasa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.Infrastructure;

public static class DependencyInjection
{
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="connectionString">
    /// Fornece a string de conexão. É uma função para que o valor seja lido só quando
    /// o banco for usado, depois que toda a configuração (inclusive a dos testes) estiver carregada.
    /// </param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Func<IServiceProvider, string> connectionString)
    {
        services.AddDbContext<AppDbContext>((provider, options) =>
            options
                .UseNpgsql(connectionString(provider))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // Senha: comprimento mínimo, sem regras de composição arbitrárias.
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                // Bloqueio temporário após falhas seguidas de login.
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUserAccounts, UserAccounts>();

        return services;
    }
}
