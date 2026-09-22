using Matgar.Application.Abstractions.Caching;
using Matgar.Application.Abstractions.Persistence.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.SuspendProduct
{
    public class SuspendProductCommandHandler : IRequestHandler<SuspendProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public SuspendProductCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<Result> Handle(SuspendProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");


            if (product.Status == ProductStatus.Suspended)
                return Error.Conflict(
                    code: "Product.AlreadySuspended",
                    message: "Product is already suspended.");

            product.Status = ProductStatus.Suspended;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveByPrefixAsync("GetProducts", cancellationToken);
            await _cacheService.RemoveByPrefixAsync("GetProductById", cancellationToken);

            return Result.Success;
        }
    }
}
