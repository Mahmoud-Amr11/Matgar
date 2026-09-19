namespace Matgar.Application.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void New_order_should_default_to_pending()
    {
        new Order().Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void New_order_should_default_discount_to_zero()
    {
        new Order().DiscountAmount.Should().Be(0m);
    }
}

public class ProductTests
{
    [Fact]
    public void New_product_should_default_to_draft()
    {
        new Product().Status.Should().Be(ProductStatus.Draft);
    }
}

public class PaymentTests
{
    [Fact]
    public void New_payment_should_default_to_pending()
    {
        new Payment().Status.Should().Be(PaymentStatus.Pending);
    }
}