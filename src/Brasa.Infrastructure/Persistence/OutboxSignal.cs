using Brasa.Application.Shared.Jobs;

namespace Brasa.Infrastructure.Persistence;

public sealed class OutboxSignal() : JobSignal(FallbackInterval)
{
    public static readonly TimeSpan FallbackInterval = TimeSpan.FromSeconds(30);
}
