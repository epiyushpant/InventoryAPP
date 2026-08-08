using System.ComponentModel.DataAnnotations;

using Inventory.Services;

namespace Inventory.Models
{
    public class StockTransfer : ITenantScoped
    {
        public int TenantId { get; set; } = 1;

        [Key]
        public int TransferID { get; set; }

        [Required]
        public int FromLocationID { get; set; }

        [Required]
        public int ToLocationID { get; set; }

        [Required]
        public int ProductID { get; set; }

        public int Quantity { get; set; }

        public DateTime TransferDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Transit, Completed, Cancelled
    }
}
