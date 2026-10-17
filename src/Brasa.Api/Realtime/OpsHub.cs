using Brasa.Application.Identity;

namespace Brasa.Api.Realtime;

internal sealed class OpsHub(IUserAccounts accounts) : StaffHub(Permissions.OrdersRead, accounts);
