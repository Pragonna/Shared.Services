namespace Shared.Services.BuildingBlocks.Exceptions;

public sealed class EventBusPublishException : Exception
{
    public EventBusPublishException(string message, Exception innerException = null)
        : base(message, innerException)
    {
    }
}