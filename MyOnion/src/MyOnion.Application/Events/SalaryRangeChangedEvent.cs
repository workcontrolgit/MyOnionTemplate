#nullable enable
namespace MyOnion.Application.Events;

public sealed record SalaryRangeChangedEvent(Guid SalaryRangeId) : IDomainEvent;
