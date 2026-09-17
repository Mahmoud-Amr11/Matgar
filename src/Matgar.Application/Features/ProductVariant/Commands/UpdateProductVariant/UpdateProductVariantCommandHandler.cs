using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common;
using Matgar.Application.Common.Caching;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.ProductVariant.Commands.UpdateProductVariant
{
    public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ICacheService _cacheService;

        public UpdateProductVariantCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _cacheService = cacheService;
        }

        public async Task<Result> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
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

            // منع تحويل الفاريانت لتركيبة مكررة (نفس Product + نفس الأتريبيوتس)
            // مع استثناء الفاريانت نفسه من المقارنة.
            var existingVariants = await _unitOfWork.ProductVariants.FindAsync(
                v => v.ProductId == request.ProductId && v.Id != request.VariantId, cancellationToken);
            var newCombination = VariantAttributeComparer.Canonical(request.AttributesJson);
            if (existingVariants.Any(v => string.Equals(VariantAttributeComparer.Canonical(v.AttributesJson), newCombination, StringComparison.Ordinal)))
                return Error.Conflict(code: "Variant.CombinationExists", message: "A variant with these attributes already exists for this product.");

            variant.Price = request.Price;
            variant.ImageUrl = request.ImageUrl;
            variant.AttributesJson = request.AttributesJson;

            _unitOfWork.ProductVariants.Update(variant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync($"GetProductVariants_{request.ProductId}", cancellationToken);

            return Result.Success;
        }
    }
}
