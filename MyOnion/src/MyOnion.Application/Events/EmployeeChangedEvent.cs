#nullable enable
namespace MyOnion.Application.Events;

public sealed record EmployeeChangedEvent(Guid EmployeeId) : IDomainEvent;
