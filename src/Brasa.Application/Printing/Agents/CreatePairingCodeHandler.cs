using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Auth;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Agents;

public sealed class CreatePairingCodeHandler(IAppDbContext db, CurrentUser currentUser, AuditTrail audit, TimeProvider clock)
{
    public async Task<PairingCodeResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var (pairingCode, plainCode) = PairingCode.Generate(currentUser.UserId, clock.GetUtcNow());

        db.PairingCodes.Add(pairingCode);
        audit.Record("printing.pairing_code_created", pairingCode.Id);
        await db.SaveChangesAsync(cancellationToken);

        return new PairingCodeResponse($"{plainCode[..4]}-{plainCode[4..]}", pairingCode.ExpiresAt);
    }
}
