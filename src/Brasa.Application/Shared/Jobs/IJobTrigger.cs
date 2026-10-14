namespace Brasa.Application.Shared.Jobs;

public interface IJobTrigger
{
    Task WaitAsync(CancellationToken cancellationToken);
}
