using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;
using System.Text.RegularExpressions;

namespace Matgar.Application.Features.Category.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var nameAlreadyExists = await _unitOfWork.Categories.AnyAsync(c => c.Name == request.Name);
            if (nameAlreadyExists)
                return Error.Conflict("Category.NameExists", "This name already exist.");



            var category = new Domain.Entities.Category
            {
                Name = request.Name,
                Slug = GenerateSlug(request.Name)
            };

            await _unitOfWork.Categories.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }

        private string GenerateSlug(string name)
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
