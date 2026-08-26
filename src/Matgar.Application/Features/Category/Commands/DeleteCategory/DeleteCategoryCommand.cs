using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Category.Commands.DeleteCategory
{
    public sealed record DeleteCategoryCommand(Guid CategoryId) : IRequest<Result>;

}
