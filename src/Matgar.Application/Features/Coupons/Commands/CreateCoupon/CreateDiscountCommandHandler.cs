using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Coupons.Commands.CreateCoupon
{
    public class CreateDiscountCommandHandler : IRequestHandler<CreateDiscountCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        public CreateDiscountCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var adminId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            var code = await _unitOfWork.Coupons.AnyAsync(c => c.Code == request.Code, cancellationToken);

            if (code) return Error.Conflict("Coupon code already exists", "A coupon with the same code already exists.");

            var coupon = new Domain.Entities.Coupon
            {
                Code = request.Code,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MinOrderAmount = request.MinOrderAmount,
                MaxUsageCount = request.MaxUsageCount,
                ExpiryDate = request.ExpiryDate
            };

            await _unitOfWork.Coupons.AddAsync(coupon, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(coupon.Id);
        }
    }
}

