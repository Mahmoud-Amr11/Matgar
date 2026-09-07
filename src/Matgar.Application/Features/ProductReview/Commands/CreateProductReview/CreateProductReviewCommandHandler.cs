using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Queries.ProductReview;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.ProductReview.Commands.CreateProductReview
{
    public class CreateProductReviewCommandHandler : IRequestHandler<CreateProductReviewCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IPurchaseVerificationQueries _purchaseVerification;

        public CreateProductReviewCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IPurchaseVerificationQueries purchaseVerification)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _purchaseVerification = purchaseVerification;
        }

        public async Task<Result<Guid>> Handle(CreateProductReviewCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");

            var orderId = await _purchaseVerification.FindReviewableDeliveredOrderIdAsync(
                userId, request.ProductId, cancellationToken);

            if (orderId is null)
                return Error.Forbidden(
                    code: "Review.PurchaseNotVerified",
                    message: "You can only review products from a delivered order you purchased, and you cannot review the same order twice.");

            var review = new Domain.Entities.ProductReview
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                UserId = userId,
                OrderId = orderId.Value,
                Rating = request.Rating,
                Comment = request.Comment
            };

            await _unitOfWork.ProductReviews.AddAsync(review, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return review.Id;
        }
    }
}
