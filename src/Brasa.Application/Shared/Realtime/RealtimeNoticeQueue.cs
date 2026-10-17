using Brasa.Application.Shared.Outbox;
using Brasa.Application.Shared.Persistence;

namespace Brasa.Application.Shared.Realtime;

public static class RealtimeNoticeQueue
{
    public static void QueueRealtimeNotice(this IAppDbContext db, string channel, string type, Guid subjectId, DateTimeOffset now)
    {
        var alreadyQueued = db.OutboxMessages.Local.Any(message =>
            message.ProcessedAt is null && message.Channel == channel && message.Type == type && message.SubjectId == subjectId);

        if (!alreadyQueued)
        {
            db.OutboxMessages.Add(new OutboxMessage(channel, type, subjectId, now));
        }
    }
}
