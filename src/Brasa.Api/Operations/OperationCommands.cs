using Brasa.Application.Identity.Team;
using Brasa.Infrastructure.Persistence;

namespace Brasa.Api.Operations;

internal static class OperationCommands
{
    public const string Migrate = "migrate";
    public const string InviteOwner = "invite-owner";
    public const string AdminPublicUrlSetting = "Admin:PublicUrl";

    public static async Task<bool> TryRunAsync(WebApplication app, string[] args)
    {
        switch (args)
        {
            case [Migrate]:
                var store = app.Configuration.GetSection(StoreSetupOptions.Section).Get<StoreSetupOptions>() ?? new StoreSetupOptions();
                await ProductionDatabase.MigrateAsync(app.Services, store);
                Console.WriteLine("Banco atualizado.");
                return true;

            case [InviteOwner, var email, var displayName]:
                await InviteOwnerAsync(app, email, displayName);
                return true;

            case [Migrate or InviteOwner, ..]:
                throw new ArgumentException($"Uso: '{Migrate}' ou '{InviteOwner} <e-mail> <nome>'.");

            default:
                return false;
        }
    }

    private static async Task InviteOwnerAsync(WebApplication app, string email, string displayName)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var invited = await scope.ServiceProvider.GetRequiredService<InviteOwnerHandler>().HandleAsync(email, displayName, CancellationToken.None);
        if (!invited.IsSuccess)
        {
            throw new InvalidOperationException(invited.Error!.Message);
        }

        var adminUrl = app.Configuration[AdminPublicUrlSetting]?.TrimEnd('/') ?? string.Empty;
        Console.WriteLine($"Convite do proprietário criado. Válido até {invited.Value!.ExpiresAt:dd/MM/yyyy HH:mm} (UTC).");
        Console.WriteLine($"{adminUrl}/convite/{invited.Value.Token}");
    }
}
