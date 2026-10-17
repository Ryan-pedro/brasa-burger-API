using Brasa.Application.Shared.Jobs;

namespace Brasa.Api.Jobs;

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
