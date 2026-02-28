using MediatR;
using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Queries;

public interface IQueryHandler<TRequest, TResponse> :
    IRequestHandler<TRequest, Result<TResponse, Error>>
    where TRequest : IQuery<TResponse>
    where TResponse : notnull
{
}