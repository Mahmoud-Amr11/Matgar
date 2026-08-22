using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Category.Commands.CreateCategory
{
    public sealed record CreateCategoryCommand(string Name) : IRequest<Result>;
}
