namespace Brasa.Domain.Shared;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _events = [];

    public IReadOnlyList<IDomainEvent> TakeEvents()
    {
        var events = _events.ToList();
        _events.Clear();
        return events;
    }

    protected void Raise(IDomainEvent domainEvent) => _events.Add(domainEvent);
}
