namespace Shared.Services.BuildingBlocks.Exceptions;

public sealed class EventBusConnectionException : Exception
{
    public EventBusConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}