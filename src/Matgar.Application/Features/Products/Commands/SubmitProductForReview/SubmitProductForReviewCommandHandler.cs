using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.SubmitProductForReview
{
    public class SubmitProductForReviewCommandHandler : IRequestHandler<SubmitProductForReviewCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public SubmitProductForReviewCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(SubmitProductForReviewCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var vendorId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");

            if (product.VendorId != vendorId)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");


            if (product.Status != ProductStatus.Draft)
                return Error.Conflict(
                    code: "Product.InvalidStatusTransition",
                    message: $"Cannot submit for review. Product is currently '{product.Status}', expected 'Draft'.");

            product.Status = ProductStatus.PendingReview;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}

