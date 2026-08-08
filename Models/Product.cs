using System.ComponentModel.DataAnnotations;

using Inventory.Services;

namespace Inventory.Models
{
    public class Product : ITenantScoped
    {
        public int TenantId { get; set; } = 1;

        [Key]
        public int ProductID { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        public int? CategoryID { get; set; }
        public int? SupplierID { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(20)]
        public string? UnitOfMeasure { get; set; } // PCS, KG, LTR, etc.

        public decimal CostPrice { get; set; }
        public decimal UnitPrice { get; set; } // Selling Price

        public int? ReorderLevel { get; set; }
        public int? MaxStockLevel { get; set; }

        // Physical attributes (used for shipping / storage planning)
        public decimal? WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }

        [StringLength(60)]
        public string? WarrantyPeriod { get; set; } // e.g. "12 Months"

        /// <summary>When true, line amounts attract 13% VAT on sales invoices.</summary>
        public bool IsTaxable { get; set; } = true;
        
        public bool IsActive { get; set; } = true;
    }
}
