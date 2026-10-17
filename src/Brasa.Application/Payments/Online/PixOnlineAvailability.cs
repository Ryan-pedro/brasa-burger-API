using Brasa.Domain.StoreSettings;

namespace Brasa.Application.Payments.Online;

public static class PixOnlineAvailability
{
    public static bool IsOfferedBy(Store store, IPixGateway gateway) => store.AcceptsOnlinePix && gateway.IsConfigured;
}
