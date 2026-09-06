using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateProductCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var vendorId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");


            if (product.VendorId != vendorId)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");

            var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
                return Error.NotFound(code: "Category.NotFound", message: "Category not found.");

            product.Name = request.Name;
            product.Description = request.Description;
            product.CategoryId = request.CategoryId;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
