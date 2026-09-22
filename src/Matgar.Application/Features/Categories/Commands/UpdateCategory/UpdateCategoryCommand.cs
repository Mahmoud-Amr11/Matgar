using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Category.Commands.UpdateCategory
{
    public sealed record UpdateCategoryCommand(Guid CategoryId, string NewName) : IRequest<Result>;
}
