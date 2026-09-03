using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Caching;
using Matgar.Application.Common.Results;
using MediatR;
using System.Text.RegularExpressions;

namespace Matgar.Application.Features.Category.Commands.UpdateCategory
{
    public class UpdateCategoryComandHandler : IRequestHandler<UpdateCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public UpdateCategoryComandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);

            if (category == null)
                return Error.NotFound(message: "Category is not found.");

            if (string.Equals(category.Name, request.NewName, StringComparison.OrdinalIgnoreCase))
                return Error.Failure(message: "The new name is the same as the current name.");

            var newSlug = GenerateSlug(request.NewName);

            var slugExists = await _unitOfWork.Categories.AnyAsync(
                c => c.Slug == newSlug && c.Id != request.CategoryId,
                cancellationToken);

            if (slugExists)
                return Error.Conflict(message: "A category with this name already exists.");

            var oldSlug = category.Slug;

            category.Name = request.NewName;
            category.Slug = newSlug;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync($"category-id:{request.CategoryId}");
            await _cacheService.RemoveAsync($"category-slug:{oldSlug}");
            await _cacheService.RemoveAsync("categories:all");

            return Result.Success;
        }

        private static string GenerateSlug(string name)
        {
            var slug = name.ToLowerInvariant().Trim();

            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}