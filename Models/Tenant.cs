using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Models
{
    public class Tenant
    {
        [Key]
        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "Default Shop";

        /// <summary>Kirana | Pharmacy | Wholesale | Full</summary>
        [Required]
        [StringLength(40)]
        public string Preset { get; set; } = "Full";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TenantCapability> Capabilities { get; set; } = new List<TenantCapability>();
    }

    public class TenantCapability
    {
        [Key]
        public int TenantCapabilityId { get; set; }

        public int TenantId { get; set; }

        [ForeignKey(nameof(TenantId))]
        public Tenant? Tenant { get; set; }

        /// <summary>Catalog key e.g. module.sales, form.grns, report.vat-sales-register</summary>
        [Required]
        [StringLength(120)]
        public string Key { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        /// <summary>For field keys: mark required when visible</summary>
        public bool? Required { get; set; }

        public bool? ReadOnly { get; set; }
    }
}
