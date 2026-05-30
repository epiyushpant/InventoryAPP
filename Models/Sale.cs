using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Sale
    {
        [Key]
        public int SaleID { get; set; } // SO ID

        [Required]
        public int CustomerID { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow; // Order Date

        public int? LocationID { get; set; } // Warehouse ID

        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Confirmed, Shipped, Closed

        public decimal TotalAmount { get; set; }

        public ICollection<SaleDetail>? SaleDetails { get; set; }
    }
}
