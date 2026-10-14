using Brasa.Api.Auth;
using Brasa.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Brasa.Api.Realtime;

[Authorize(AdminAuth.Scheme)]
internal abstract class StaffHub(string permission, IUserAccounts accounts) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var allowed = AdminAuth.TryReadUserId(Context.User!, out var userId)
            && await accounts.FindAsync(userId, Context.ConnectionAborted) is { IsActive: true } user
            && Permissions.For(user.Role).Contains(permission);

        if (!allowed)
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }
}

internal sealed class KitchenHub(IUserAccounts accounts) : StaffHub(Permissions.KitchenTicketsRead, accounts);

internal sealed class OpsHub(IUserAccounts accounts) : StaffHub(Permissions.OrdersRead, accounts);
