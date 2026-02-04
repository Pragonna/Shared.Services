namespace Shared.Services.BuildingBlocks.EventBus.Base.Events;

public interface IIntegrationEventHandler<TEvent>
    where TEvent : IIntegrationEvent
{
    Task Handle(TEvent @event);
}