using EventBus.EventBus.Base;
using Shared.Services.BuildingBlocks.EventBus.Base.Abstraction;
using Shared.Services.BuildingBlocks.EventBus.Base.Events;

namespace Shared.Services.BuildingBlocks.EventBus.Base;

 public class EventBusSubscriptionManager : IEventSubscriptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> handlers;
        private readonly List<Type> eventTypes;

        public event EventHandler<string> OnEventRemoved;
        public Func<string, string> eventNameGetter;

        public EventBusSubscriptionManager(Func<string, string> eventNameGetter)
        {
            handlers = new Dictionary<string, List<SubscriptionInfo>>();
            eventTypes = new List<Type>();
            this.eventNameGetter = eventNameGetter;
        }


        public bool IsEmpty => !handlers.Keys.Any();
        public void Clear() => handlers.Clear();

        public void AddSubscription<T, THandle>()
            where T : IntegrationEvent
            where THandle : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            AddSubscription(typeof(THandle), eventName);

            if (!eventTypes.Contains(typeof(T)))
            {
                eventTypes.Add(typeof(T));
            }
        }
        private void AddSubscription(Type handlerType, string eventName)
        {
            if (!HasSubscriptionForEvent(eventName))
            {
                handlers.Add(eventName, new List<SubscriptionInfo>());
            }
            if (handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new InvalidOperationException($"Handler type {handlerType.Name} already registered for `{eventName}`");
            }
            handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));
        }

        public string GetEventKey<T>()
        {
            string eventName = typeof(T).Name;
            return eventNameGetter(eventName);
        }

        public Type GetEventTypeName(string eventName) => eventTypes.SingleOrDefault(t => t.Name == eventName);


        public bool HasSubscriptionForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();

            return HasSubscriptionForEvent(key);
        }

        public bool HasSubscriptionForEvent(string eventName) => handlers.ContainsKey(eventName);

        public void RemoveSubscription<T, THandle>()
            where T : IntegrationEvent
            where THandle : IIntegrationEventHandler<T>
        {
            var handlerToRemove = FindSubscriptionRemove<T, THandle>();
            var eventName = GetEventKey<T>();
            RemoveHandler(eventName, handlerToRemove);
        }

        private void RemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                handlers[eventName].Remove(subsToRemove);

                if (!handlers[eventName].Any())
                {
                    handlers.Remove(eventName);
                    var eventType = eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        eventTypes.Remove(eventType);
                    }
                }
                RaiseOnEventRemove(eventName);
            }
        }

        private void RaiseOnEventRemove(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }

        private SubscriptionInfo FindSubscriptionRemove<T, THandle>()
            where T : IntegrationEvent
            where THandle : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return FindSubscriptionRemove(eventName, typeof(THandle));
        }

        private SubscriptionInfo FindSubscriptionRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionForEvent(eventName))
            {
                return null; // we need a subscriptionInfo does not matter it will be null or not null. 
            }
            return handlers[eventName].SingleOrDefault(e => e.HandlerType == handlerType);
        }
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => handlers[eventName];
    }