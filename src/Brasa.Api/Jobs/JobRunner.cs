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

internal sealed class IntervalTrigger(TimeSpan interval) : IJobTrigger
{
    public Task WaitAsync(CancellationToken cancellationToken) => Task.Delay(interval, cancellationToken);
}

internal static class JobRegistration
{
    public const string EnabledSetting = "Jobs:Enabled";

    public static IServiceCollection AddJob<TJob>(this IServiceCollection services, TimeSpan interval)
        where TJob : IJob =>
        services.AddJob<TJob>(_ => new IntervalTrigger(interval));

    public static IServiceCollection AddJob<TJob>(this IServiceCollection services, Func<IServiceProvider, IJobTrigger> trigger)
        where TJob : IJob =>
        services.AddHostedService(provider => new JobRunner<TJob>(
            provider.GetRequiredService<IServiceScopeFactory>(),
            trigger(provider),
            provider.GetRequiredService<ILogger<JobRunner<TJob>>>()));
}
