using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class DeliveryNote
    {
        [Key]
        public int DeliveryID { get; set; }

        [Required]
        public int SaleID { get; set; }

        [Required]
        public int ProductID { get; set; }

        public int ShippedQuantity { get; set; }

        public DateTime ShipmentDate { get; set; } = DateTime.UtcNow;

        public string? TransportDetails { get; set; }
    }
}
