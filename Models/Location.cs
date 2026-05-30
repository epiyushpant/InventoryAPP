using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Location
    {
        [Key]
        public int LocationID { get; set; }

        [Required]
        [StringLength(100)]
        public string WarehouseName { get; set; } = string.Empty;

        public string? Address { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(100)]
        public string? ManagerName { get; set; }

        [StringLength(50)]
        public string? ContactNo { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Type { get; set; } // e.g. Warehouse, Storefront
    }
}
