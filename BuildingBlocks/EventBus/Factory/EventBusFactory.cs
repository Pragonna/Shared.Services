using EventBus.RabbitMQ;
using Shared.Services.BuildingBlocks.EventBus.Base;
using Shared.Services.BuildingBlocks.EventBus.Base.Abstraction;
using Shared.Services.BuildingBlocks.EventBus.Base.Enums;

namespace EventBus.Factory;

public class EventBusFactory
{
    public static IEventBus Create(EventBusConfig config, IServiceProvider provider)
    {
        return config.EventBusType switch
        {
            EventBusType.RabbitMQ => new EventBusRabbitMQ(provider, config)
        };
    }
}