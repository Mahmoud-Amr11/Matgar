using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Category.Commands.UpdateCategory
{
    public sealed record UpdateCategoryCommmand(Guid CategoryId) : IRequest<Result>;
}
