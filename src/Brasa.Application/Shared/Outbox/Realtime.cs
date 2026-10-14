namespace Brasa.Application.Shared.Outbox;

public static class RealtimeChannels
{
    public const string Kitchen = "kitchen";
    public const string Ops = "ops";
}

public static class RealtimeTypes
{
    public const string OrderChanged = "order.changed";
    public const string TicketChanged = "ticket.changed";
}

public sealed record RealtimeNotice(string Type, Guid Id);

public interface IRealtimeNotifier
{
    Task NotifyAsync(string channel, RealtimeNotice notice, CancellationToken cancellationToken);
}
