using Brasa.Application.Shared.Jobs;

namespace Brasa.Api.Jobs;

internal sealed class IntervalTrigger(TimeSpan interval) : IJobTrigger
{
    public Task WaitAsync(CancellationToken cancellationToken) => Task.Delay(interval, cancellationToken);
}
