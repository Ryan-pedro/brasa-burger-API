using System.Threading.Channels;

namespace Brasa.Application.Shared.Jobs;

public class JobSignal(TimeSpan fallbackInterval) : IJobTrigger
{
    private readonly Channel<bool> _pending = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

    public void Notify() => _pending.Writer.TryWrite(true);

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAny(_pending.Reader.WaitToReadAsync(cancellationToken).AsTask(), Task.Delay(fallbackInterval, cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();
        _pending.Reader.TryRead(out _);
    }
}
