using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Models
{
    /// <summary>
    /// What one role may use inside one shop. Deliberately not ITenantScoped — like
    /// TenantCapability, these rows are written for another tenant during provisioning, so the
    /// global filter would hide them at exactly the wrong moment.
    /// </summary>
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        public int TenantId { get; set; }

        [ForeignKey(nameof(TenantId))]
        public Tenant? Tenant { get; set; }

        [Required]
        [StringLength(60)]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>Catalog key, e.g. form.grns or report.low-stock.</summary>
        [Required]
        [StringLength(120)]
        public string Key { get; set; } = string.Empty;

        public bool Allowed { get; set; } = true;
    }
}
