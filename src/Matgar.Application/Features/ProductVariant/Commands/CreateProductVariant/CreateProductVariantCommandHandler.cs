using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Domain.Entities;
using MediatR;

namespace Matgar.Application.Features.ProductVariant.Commands.CreateProductVariant
{
    public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CreateProductVariantCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var vendorId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");

            if (product.VendorId != vendorId)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");

            var skuExists = await _unitOfWork.ProductVariants.AnyAsync(v => v.Sku == request.Sku, cancellationToken);
            if (skuExists)
                return Error.Conflict(code: "Variant.SkuExists", message: "This SKU is already in use.");

            var variant = new Domain.Entities.ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Sku = request.Sku,
                Price = request.Price,
                ImageUrl = request.ImageUrl,
                AttributesJson = request.AttributesJson,
                StockItem = new StockItem
                {
                    Id = Guid.NewGuid(),
                    QuantityOnHand = request.InitialQuantity,
                    QuantityReserved = 0
                }
            };

            await _unitOfWork.ProductVariants.AddAsync(variant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return variant.Id;
        }
    }
}
