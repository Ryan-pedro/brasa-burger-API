using Brasa.Domain.Identity;

namespace Brasa.Application.Identity;

/// <summary>
/// Permissões do sistema e o que cada papel pode fazer.
/// Os papéis não se herdam entre interfaces: gerente não ganha as permissões da
/// cozinha, nem a cozinha as do gerente.
/// </summary>
public static class Permissions
{
    public const string OrdersRead = "orders.read";
    public const string OrdersManage = "orders.manage";
    public const string CatalogRead = "catalog.read";
    public const string CatalogWrite = "catalog.write";
    public const string OffersWrite = "offers.write";
    public const string StoreSettings = "store.settings";
    public const string PrintingManage = "printing.manage";
    public const string TeamManage = "team.manage";

    public const string KitchenTicketsRead = "kitchen.tickets.read";
    public const string KitchenTicketsUpdate = "kitchen.tickets.update";
    public const string KitchenTicketsReprint = "kitchen.tickets.reprint";

    private static readonly string[] Manager =
    [
        OrdersRead, OrdersManage, CatalogRead, CatalogWrite, OffersWrite, StoreSettings, PrintingManage,
    ];

    private static readonly string[] Owner = [.. Manager, TeamManage];

    private static readonly string[] Kitchen = [KitchenTicketsRead, KitchenTicketsUpdate, KitchenTicketsReprint];

    public static IReadOnlyList<string> For(StaffRole role) => role switch
    {
        StaffRole.Owner => Owner,
        StaffRole.Manager => Manager,
        StaffRole.Kitchen => Kitchen,
        _ => [],
    };
}
