using System.Collections.Concurrent;
using System.Reflection;
using Brasa.Application.Shared.Events;
using Brasa.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.Infrastructure.Persistence;

public sealed class DomainEventDispatcher(IServiceProvider services)
{
    private const int MaxRounds = 10;

    private static readonly ConcurrentDictionary<Type, (Type HandlerType, MethodInfo Handle)> Handlers = new();

    public async Task DispatchAsync(DbContext context, CancellationToken cancellationToken)
    {
        for (var round = 0; ; round++)
        {
            var events = context.ChangeTracker
                .Entries<AggregateRoot>()
                .SelectMany(entry => entry.Entity.TakeEvents())
                .ToList();

            if (events.Count == 0)
            {
                return;
            }

            if (round == MaxRounds)
            {
                throw new InvalidOperationException("Os eventos de domínio não convergiram: verifique reações em ciclo.");
            }

            foreach (var domainEvent in events)
            {
                await PublishAsync(domainEvent, cancellationToken);
            }
        }
    }

    private async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var (handlerType, handle) = Handlers.GetOrAdd(domainEvent.GetType(), eventType =>
        {
            var type = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            return (type, type.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!);
        });

        foreach (var handler in services.GetServices(handlerType))
        {
            await (Task)handle.Invoke(handler, [domainEvent, cancellationToken])!;
        }
    }
}
