using System.ComponentModel.DataAnnotations;

using Inventory.Services;

namespace Inventory.Models
{
    public class Sale : ITenantScoped
    {
        public int TenantId { get; set; } = 1;

        [Key]
        public int SaleID { get; set; } // SO ID

        [Required]
        public int CustomerID { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow; // Order Date

        public int? LocationID { get; set; } // Warehouse ID

        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Pending, Confirmed, Completed, Cancelled (legacy: Shipped, Closed = locked)

        public decimal TotalAmount { get; set; }

        public ICollection<SaleDetail>? SaleDetails { get; set; }
    }
}
