using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.ProductReview.Commands.CreateProductReview
{
    public sealed record CreateProductReviewCommand(
        Guid ProductId,
        int Rating,
        string? Comment) : IRequest<Result<Guid>>;
}
