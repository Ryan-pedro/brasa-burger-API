using System.Text.Json;
using Brasa.Application.Shared.Auth;
using Brasa.Application.Shared.Persistence;

namespace Brasa.Application.Shared.Audit;

public sealed class AuditTrail(IAppDbContext db, CurrentUser currentUser, TimeProvider clock)
{
    public void Record(string action, Guid? subjectId, object? data = null, Guid? userId = null) =>
        db.AuditEntries.Add(new AuditEntry(
            action,
            subjectId,
            userId ?? currentUser.UserId,
            data is null ? null : JsonSerializer.Serialize(data, JsonSerializerOptions.Web),
            clock.GetUtcNow()));
}
