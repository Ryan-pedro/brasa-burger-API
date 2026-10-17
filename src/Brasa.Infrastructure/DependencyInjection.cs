using Brasa.Application.Catalog.Images;
using Brasa.Application.Identity;
using Brasa.Application.Ordering.Checkout;
using Brasa.Application.Payments.Online;
using Brasa.Application.Printing.Jobs;
using Brasa.Application.Shared.Outbox;
using Brasa.Application.Shared.Persistence;
using Brasa.Infrastructure.Identity;
using Brasa.Infrastructure.Payments;
using Brasa.Infrastructure.Persistence;
using Brasa.Infrastructure.Storage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Brasa.Infrastructure;

public static class DependencyInjection
{
    public const string DataProtectionApplicationName = "brasa";

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
        services.AddScoped<DomainEventDispatcher>();
        services.AddSingleton<OutboxSignal>();
        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IOrderNumbers, OrderNumbers>();
        services.AddScoped<IPrintJobQueue, PrintJobQueue>();
        services.AddSingleton<IDatabaseErrors, DatabaseErrors>();
        services.TryAddSingleton(TimeProvider.System);

        services.AddOptions<ImageStorageOptions>().BindConfiguration(ImageStorageOptions.Section);
        services.AddSingleton<IImageStorage, LocalImageStorage>();

        services.AddOptions<PixGatewayOptions>().BindConfiguration(PixGatewayOptions.Section);
        services.AddSingleton<FakePixGateway>();
        services.AddSingleton<NotConfiguredPixGateway>();
        services.AddSingleton<IPixGateway>(provider => provider.GetRequiredService<IOptions<PixGatewayOptions>>().Value.Provider switch
        {
            null or "" => provider.GetRequiredService<NotConfiguredPixGateway>(),
            PixGatewayOptions.FakeProvider => provider.GetRequiredService<FakePixGateway>(),
            var unknown => throw new InvalidOperationException($"Gateway de Pix desconhecido: {unknown}."),
        });

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = IdentityRules.PasswordMinimumLength;
                options.Password.RequiredUniqueChars = IdentityRules.PasswordMinimumUniqueCharacters;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = IdentityRules.MaxFailedLoginAttempts;
                options.Lockout.DefaultLockoutTimeSpan = IdentityRules.LockoutDuration;
            })
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUserAccounts, UserAccounts>();

        services.AddDataProtection()
            .SetApplicationName(DataProtectionApplicationName)
            .PersistKeysToDbContext<AppDbContext>();

        return services;
    }
}
