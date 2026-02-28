using MediatR;
using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse, Error>>, IBaseRequest where TResponse : notnull
{
}