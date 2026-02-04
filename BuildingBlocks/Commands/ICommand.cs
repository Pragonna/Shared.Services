using MediatR;
using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Commands;

public interface ICommand<TResponse> : IRequest<Result<TResponse, Error>>
    where TResponse : notnull
{
}