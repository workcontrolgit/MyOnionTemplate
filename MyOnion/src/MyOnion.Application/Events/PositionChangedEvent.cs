#nullable enable
namespace MyOnion.Application.Events;

public sealed record PositionChangedEvent(Guid PositionId) : IDomainEvent;
