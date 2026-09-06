using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.ApproveProduct
{
    public class ApproveProductCommandHandler : IRequestHandler<ApproveProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApproveProductCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ApproveProductCommand request, CancellationToken cancellationToken)
        {

            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");

            if (product.Status != ProductStatus.PendingReview)
                return Error.Conflict(
                    code: "Product.InvalidStatusTransition",
                    message: $"Cannot approve. Product is currently '{product.Status}', expected 'PendingReview'.");

            product.Status = ProductStatus.Active;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
