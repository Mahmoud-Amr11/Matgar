using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Caching;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Category.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
            if (category is null)
                return Error.NotFound(message: "Category is not found please try again.");

            //var hasProducts = await _unitOfWork.Products.AnyAsync(p => p.CategoryId == request.CategoryId);
            //if (hasProducts)
            //    return Error.Conflict("Category.HasProducts", "Cannot delete this category because it has associated products.");

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _cacheService.RemoveAsync($"category-id:{request.CategoryId}");
            await _cacheService.RemoveAsync("categories:all");
            return Result.Success;
        }
    }

}
