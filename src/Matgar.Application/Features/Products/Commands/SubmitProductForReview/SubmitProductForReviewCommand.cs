using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.SubmitProductForReview
{
    public sealed record SubmitProductForReviewCommand(Guid ProductId) : IRequest<Result>;
}

