using Newtonsoft.Json;

namespace Shared.Services.BuildingBlocks.EventBus.Base.Events;

public interface IIntegrationEvent
{
}

public record IntegrationEvent : IIntegrationEvent
{
    [JsonProperty] public Guid Id { get; private set; }
    [JsonProperty] public DateTime CreatedAt { get; private set; }

    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    [Newtonsoft.Json.JsonConstructor]
    public IntegrationEvent(Guid id, DateTime atTime)
    {
        Id = id;
        CreatedAt = atTime;
    }
}