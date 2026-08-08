using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matgar.Domain.Entities
{
    public class StockItem
    {
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();


        public ProductVariant ProductVariant { get; set; } = null!;


        [NotMapped]
        public int AvailableQuantity => QuantityOnHand - QuantityReserved;
    }
}