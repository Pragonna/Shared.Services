using EventBus.EventBus.Base;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shared.Services.BuildingBlocks.EventBus.Base.Abstraction;
using Shared.Services.BuildingBlocks.EventBus.Base.Events;

namespace Shared.Services.BuildingBlocks.EventBus.Base
{
    public abstract class BaseEventBus : IEventBus
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly IEventSubscriptionManager SubscriptionManager;

        public EventBusConfig EventBusConfig { get; set; }

        protected BaseEventBus(IServiceProvider serviceProvider, EventBusConfig config)
        {
            EventBusConfig = config;
            ServiceProvider = serviceProvider;
            SubscriptionManager = new EventBusSubscriptionManager(ProcessEventName);
        }

        public virtual string ProcessEventName(string eventName)
        {
            if (EventBusConfig.DeleteEventPrefix)
                eventName = eventName.TrimStart(EventBusConfig.EventNamePrefix.ToArray());

            if (EventBusConfig.DeleteEventSuffix)
                eventName = eventName.TrimEnd(EventBusConfig.EventNameSuffix.ToArray());

            return eventName;
        }

        public virtual string GetSubName(string eventName)
        {
            // its queue name
            return $"{EventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }

        public virtual void Dispose()
        {
            EventBusConfig = null;
        }

        public async Task<bool> ProcessEvent(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);

            bool processed = false;

            if (SubscriptionManager.HasSubscriptionForEvent(eventName))
            {
                var subscriptions = SubscriptionManager.GetHandlersForEvent(eventName);

                using (var scope = ServiceProvider.CreateScope())
                {
                    foreach (var subscription in subscriptions)
                    {
                        var handler = ServiceProvider.GetService(subscription.HandlerType);
                        if (handler == null) continue;

                        var eventType = SubscriptionManager.GetEventTypeName(
                            $"{EventBusConfig.EventNamePrefix}{eventName}{EventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        var genericType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)genericType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }

                processed = true;
            }

            return processed;
        }

        public abstract Task Publish(IntegrationEvent @event);

        public abstract Task Subscribe<T, THandle>()
            where T : IntegrationEvent
            where THandle : IIntegrationEventHandler<T>;

        public abstract Task UnSubscribe<T, THandle>()
            where T : IntegrationEvent
            where THandle : IIntegrationEventHandler<T>;
    }
}