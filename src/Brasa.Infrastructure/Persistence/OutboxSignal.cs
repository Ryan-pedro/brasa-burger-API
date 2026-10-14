using System.Threading.Channels;
using Brasa.Application.Shared.Jobs;

namespace Brasa.Infrastructure.Persistence;

public sealed class OutboxSignal : IJobTrigger
{
    public static readonly TimeSpan FallbackInterval = TimeSpan.FromSeconds(30);

    private readonly Channel<bool> _pending = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

    public void Notify() => _pending.Writer.TryWrite(true);

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAny(_pending.Reader.WaitToReadAsync(cancellationToken).AsTask(), Task.Delay(FallbackInterval, cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();
        _pending.Reader.TryRead(out _);
    }
}
