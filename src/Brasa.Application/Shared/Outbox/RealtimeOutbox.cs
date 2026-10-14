using Brasa.Application.Shared.Persistence;

namespace Brasa.Application.Shared.Outbox;

public static class RealtimeOutbox
{
    public static void Queue(this IAppDbContext db, string channel, string type, Guid subjectId, DateTimeOffset now)
    {
        var alreadyQueued = db.OutboxMessages.Local.Any(message =>
            message.ProcessedAt is null && message.Channel == channel && message.Type == type && message.SubjectId == subjectId);

        if (!alreadyQueued)
        {
            db.OutboxMessages.Add(new OutboxMessage(channel, type, subjectId, now));
        }
    }
}
