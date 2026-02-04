
using EventBus.EventBus.Base;
using Shared.Services.BuildingBlocks.EventBus.Base.Events;

namespace Shared.Services.BuildingBlocks.EventBus.Base.Abstraction;

public interface IEventSubscriptionManager
{
    bool IsEmpty { get; }
    event EventHandler<string> OnEventRemoved;
    void AddSubscription<T, THandle>() where T : IntegrationEvent where THandle : IIntegrationEventHandler<T>;
    void RemoveSubscription<T, THandle>() where T : IntegrationEvent where THandle : IIntegrationEventHandler<T>;
    bool HasSubscriptionForEvent<T>() where T : IntegrationEvent;
    bool HasSubscriptionForEvent(string eventName);
    Type GetEventTypeName(string eventName);
    void Clear();
    IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
    IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
    string GetEventKey<T>();
}