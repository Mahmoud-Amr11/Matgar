using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.ProductVariant.Commands.DeleteProductVariant
{
    public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public DeleteProductVariantCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var vendorId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(request.VariantId, cancellationToken);
            if (variant is null)
                return Error.NotFound(code: "Variant.NotFound", message: "Variant not found.");

            if (variant.ProductId != request.ProductId)
                return Error.NotFound(code: "Variant.NotFound", message: "Variant not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(variant.ProductId, cancellationToken);
            if (product is null || product.VendorId != vendorId)
                return Error.NotFound(code: "Variant.NotFound", message: "Variant not found.");


            _unitOfWork.ProductVariants.Update(variant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
