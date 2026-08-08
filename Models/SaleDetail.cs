using System.ComponentModel.DataAnnotations;

using Inventory.Services;

namespace Inventory.Models
{
    public class SaleDetail : ITenantScoped
    {
        public int TenantId { get; set; } = 1;

        [Key]
        public int SaleDetailID { get; set; } // SO Detail ID

        [Required]
        public int SaleID { get; set; } // SO ID

        [Required]
        public int ProductID { get; set; }

        public int OrderedQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal LineTotal { get; set; }
    }
}
