using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        public int? CategoryID { get; set; }
        public int? SupplierID { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        public string? Description { get; set; }

        [StringLength(20)]
        public string? UnitOfMeasure { get; set; } // PCS, KG, LTR, etc.

        public decimal CostPrice { get; set; }
        public decimal UnitPrice { get; set; } // Selling Price

        public int? ReorderLevel { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
