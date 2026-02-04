using MediatR;
using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Commands;

public interface ICommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse, Error>>
    where TRequest : ICommand<TResponse>
    where TResponse : notnull
{
}