namespace Matgar.Application.Tests.Domain;

public class StockItemTests
{
    [Fact]
    public void AvailableQuantity_should_be_on_hand_minus_reserved()
    {
        var stock = new StockItem { QuantityOnHand = 10, QuantityReserved = 3 };

        stock.AvailableQuantity.Should().Be(7);
    }

    [Fact]
    public void AvailableQuantity_should_never_go_below_zero_when_reserved_exceeds_on_hand()
    {
        var stock = new StockItem { QuantityOnHand = 2, QuantityReserved = 5 };

        stock.AvailableQuantity.Should().Be(-3);
    }

    [Fact]
    public void New_stock_item_should_have_zero_reserved()
    {
        new StockItem { QuantityOnHand = 10 }.QuantityReserved.Should().Be(0);
    }
}