using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Abstractions.Caching
{

    public interface ICacheableQuery<T>
       : IRequest<Result<T>>
    {
        string CacheKey { get; }

        TimeSpan? Expiration { get; }

        bool BypassCache { get; }
    }
}
