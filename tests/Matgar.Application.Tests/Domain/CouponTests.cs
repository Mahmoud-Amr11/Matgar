namespace Matgar.Application.Tests.Domain;

public class CouponTests
{
    private static Coupon ValidCoupon() => new()
    {
        Code = "SAVE10",
        DiscountType = DiscountType.Percentage,
        DiscountValue = 10,
        MaxUsageCount = 5,
        UsedCount = 0,
        ExpiryDate = DateTime.UtcNow.AddDays(1),
        IsActive = true
    };

    [Fact]
    public void IsValid_should_be_true_for_active_unexpired_usable_coupon()
    {
        ValidCoupon().IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_should_be_false_when_inactive()
    {
        var coupon = ValidCoupon();
        coupon.IsActive = false;

        coupon.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_should_be_false_when_expired()
    {
        var coupon = ValidCoupon();
        coupon.ExpiryDate = DateTime.UtcNow.AddMinutes(-1);

        coupon.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_should_be_false_when_usage_limit_reached()
    {
        var coupon = ValidCoupon();
        coupon.UsedCount = coupon.MaxUsageCount;

        coupon.IsValid.Should().BeFalse();
    }
}