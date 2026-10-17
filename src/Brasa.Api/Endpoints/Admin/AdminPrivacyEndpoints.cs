using Brasa.Api.Auth;
using Brasa.Api.Problems;
using Brasa.Application.Privacy;
using Permissions = Brasa.Application.Identity.Permissions;

namespace Brasa.Api.Endpoints.Admin;

internal static class AdminPrivacyEndpoints
{
    public static RouteGroupBuilder MapAdminPrivacyEndpoints(this RouteGroupBuilder secured)
    {
        var privacy = secured.MapGroup("/privacy").RequirePermission(Permissions.PrivacyManage);

        privacy.MapPost("/personal-data-removals", async (PersonalDataRemovalRequest request, RemoveCustomerPersonalDataHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminRemoveCustomerPersonalData");

        return secured;
    }
}
