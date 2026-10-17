using Brasa.Application.Shared.Results;

namespace Brasa.Application.StoreSettings.Shared;

public static class StoreErrors
{
    public static readonly Error NotConfigured = Error.NotFound("store_not_configured", "A loja ainda não foi configurada.");
}
