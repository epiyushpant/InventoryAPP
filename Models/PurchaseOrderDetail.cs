using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class PurchaseOrderDetail
    {
        [Key]
        public int PODetailID { get; set; }

        [Required]
        public int PurchaseOrderID { get; set; }

        [Required]
        public int ProductID { get; set; }

        public int OrderedQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }
    }
}
