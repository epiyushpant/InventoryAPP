using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderID { get; set; }

        [Required]
        public int SupplierID { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedDeliveryDate { get; set; }

        public int? LocationID { get; set; } // Mapping to Warehouse ID
        
        public int? PRID { get; set; } // Link to Purchase Requisition

        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Pending, Completed, Cancelled

        public decimal TotalAmount { get; set; }

        public ICollection<PurchaseOrderDetail>? PurchaseOrderDetails { get; set; }
    }
}
