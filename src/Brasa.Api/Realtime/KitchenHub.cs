using Brasa.Application.Identity;

namespace Brasa.Api.Realtime;

internal sealed class KitchenHub(IUserAccounts accounts) : StaffHub(Permissions.KitchenTicketsRead, accounts);
