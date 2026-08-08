using System.ComponentModel.DataAnnotations;

using Inventory.Services;

namespace Inventory.Models
{
    public class Location : ITenantScoped
    {
        public int TenantId { get; set; } = 1;

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
