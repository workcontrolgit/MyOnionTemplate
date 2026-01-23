#nullable enable
namespace MyOnion.Application.Events;

public sealed record DepartmentChangedEvent(Guid DepartmentId) : IDomainEvent;
