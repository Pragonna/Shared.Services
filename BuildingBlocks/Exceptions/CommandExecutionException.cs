using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Exceptions;

public sealed class CommandExecutionException : Exception
{
    public IResult<Error> Error { get; }

    public CommandExecutionException(string message, IResult<Error> error) : base(message)
    {
        Error = error;
    }
}