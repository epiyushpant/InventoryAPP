using System.ComponentModel.DataAnnotations;

using Inventory.Services;

namespace Inventory.Models
{
    public class StockAdjustment : ITenantScoped
    {
        public int TenantId { get; set; } = 1;

        [Key]
        public int AdjustmentID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int LocationID { get; set; } // Mapping to Warehouse ID

        [Required]
        [StringLength(20)]
        public string AdjustmentType { get; set; } = "Add"; // Add, Deduct

        public int Quantity { get; set; }

        public string? Reason { get; set; }

        public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;
    }
}
