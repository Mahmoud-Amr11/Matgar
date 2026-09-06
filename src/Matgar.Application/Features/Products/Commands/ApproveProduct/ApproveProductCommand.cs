using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.ApproveProduct
{
    public sealed record ApproveProductCommand(Guid ProductId) : IRequest<Result>;
}
