namespace Brasa.Infrastructure.Persistence;

public sealed class StoreSetupOptions
{
    public const string Section = "Store";

    public string Name { get; set; } = string.Empty;

    public string TimeZoneId { get; set; } = "America/Sao_Paulo";
}
