using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CreateProductCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {

            if (!Guid.TryParse(_currentUser.UserId, out var vendorId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
                return Error.NotFound(code: "Category.NotFound", message: "Category not found.");

            var product = new Domain.Entities.Product
            {
                Id = Guid.NewGuid(),
                VendorId = vendorId,
                CategoryId = request.CategoryId,
                Name = request.Name,
                Description = request.Description,
                Status = ProductStatus.Draft
            };

            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}