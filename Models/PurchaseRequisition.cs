using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class PurchaseRequisition
    {
        [Key]
        public int PRID { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestedBy { get; set; } = string.Empty;

        [Required]
        public int ProductID { get; set; }

        public int Quantity { get; set; }

        public DateTime RequiredDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, PO Created
    }
}
