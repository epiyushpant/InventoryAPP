using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryID { get; set; }

        [Required]
        public int ProductID { get; set; }
        
        [Required]
        public int LocationID { get; set; }

        public int QuantityOnHand { get; set; }
        
        public int ReservedQuantity { get; set; }
        
        public int AvailableQuantity { get; set; }

        public DateTime? LastUpdated { get; set; }
        
        public DateTime? ExpiryDate { get; set; } // Nepal FMCG/Pharma requirement
    }
}
