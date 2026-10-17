using Brasa.Application.Shared.Jobs;

namespace Brasa.Api.Jobs;

internal sealed class JobRunner<TJob>(IServiceScopeFactory scopes, IJobTrigger trigger, ILogger<JobRunner<TJob>> logger) : BackgroundService
    where TJob : IJob
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<TJob>().RunAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Falha na tarefa {Job}.", typeof(TJob).Name);
            }

            await trigger.WaitAsync(stoppingToken);
        }
    }
}
