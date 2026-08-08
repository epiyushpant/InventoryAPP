using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Inventory.Services;

namespace Inventory.Models
{
    public class Category : ITenantScoped
    {
        public int TenantId { get; set; } = 1;

        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? ParentCategoryID { get; set; }
        
        [ForeignKey("ParentCategoryID")]
        public virtual Category? ParentCategory { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
