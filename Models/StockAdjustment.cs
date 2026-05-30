using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class StockAdjustment
    {
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
