using Shared.Services.BuildingBlocks.EventBus.Base.Events;

namespace Shared.Services.BuildingBlocks.Common;

public class Result<TResponse, TError> : IResult<TError>
    where TError : Error
{
    public TResponse Response { get; }
    public TError Error { get; }
    public bool IsSuccess { get; }
    public bool HasEvent { get; private set; }
    public ICollection<IntegrationEvent> Events { get; private set; }

    private Result()
    {
        Events = new HashSet<IntegrationEvent>();
    }

    private Result(TResponse response, IntegrationEvent @event = null) : this()
    {
        Response = response;
        IsSuccess = true;
        AddEvent(@event);
    }

    private Result(TError error, IntegrationEvent @event = null) : this()
    {
        Error = error;
        IsSuccess = false;
        AddEvent(@event);
    }

    public static Result<TResponse, TError> Success(TResponse response, IntegrationEvent @event = null)
    {
        return new Result<TResponse, TError>(response, @event);
    }

    public static Result<TResponse, TError> Success(TResponse response, IList<IntegrationEvent> events)
    {
        var result = new Result<TResponse, TError>(response);
        if (events.Any())
        {
            result.Events = events;
            result.HasEvent = true;
        }

        return result;
    }

    public static Result<TResponse, TError> Failure(TError error, IntegrationEvent @event = null)
    {
        return new Result<TResponse, TError>(error, @event);
    }

    public static Result<TResponse, TError> Failure(TError error, IList<IntegrationEvent> events)
    {
        var result = new Result<TResponse, TError>(error);
        if (events != null)
        {
            result.Events = events;
            result.HasEvent = true;
        }

        return result;
    }

    private void AddEvent(IntegrationEvent @event)
    {
        if (@event != null)
        {
            Events = Events ?? new HashSet<IntegrationEvent>();
            Events.Add(@event);
            HasEvent = true;
        }
    }
}

public interface IResult <TError> where TError : Error
{
    TError Error { get; }
    bool IsSuccess { get; }
}