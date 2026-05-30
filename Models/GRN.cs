using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class GRN
    {
        [Key]
        public int GRNID { get; set; }

        [Required]
        public int PurchaseOrderID { get; set; }

        [Required]
        public int ProductID { get; set; }

        public int ReceivedQuantity { get; set; }

        public int DamagedQuantity { get; set; }

        [Required]
        public int LocationID { get; set; } // Mapping to Warehouse ID

        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

        // Landing Cost support
        public decimal OtherExpenses { get; set; } = 0; // Transport, Loading, Custom Duty, etc.

        public DateTime? ExpiryDate { get; set; } // Nepal FMCG/Pharma requirement
    }
}
