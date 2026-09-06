using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public DeleteProductCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var vendorId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");

            if (product.VendorId != vendorId)
                return Error.NotFound(code: "Product.NotFound", message: "Product not found.");




            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
