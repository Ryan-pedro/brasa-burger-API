namespace Brasa.Application.Shared.Jobs;

public interface IJob
{
    Task RunAsync(CancellationToken cancellationToken);
}
