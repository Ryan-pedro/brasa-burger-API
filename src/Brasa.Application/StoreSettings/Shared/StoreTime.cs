using Brasa.Domain.StoreSettings;

namespace Brasa.Application.StoreSettings.Shared;

public static class StoreTime
{
    public static DateTimeOffset ToStoreTime(this Store store, DateTimeOffset moment) =>
        TimeZoneInfo.TryFindSystemTimeZoneById(store.TimeZoneId, out var timeZone)
            ? TimeZoneInfo.ConvertTime(moment, timeZone)
            : moment;
}
