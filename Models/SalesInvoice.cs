using System.ComponentModel.DataAnnotations;

using Inventory.Services;

namespace Inventory.Models
{
    public class SalesInvoice : ITenantScoped
    {
        public int TenantId { get; set; } = 1;

        [Key]
        public int InvoiceID { get; set; }

        [Required]
        public int SaleID { get; set; }

        /// <summary>Fiscal-year sequence e.g. INV-25/26-0001</summary>
        [StringLength(30)]
        public string? InvoiceNumber { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public decimal TaxAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal NonTaxableAmount { get; set; }

        public decimal GrandTotal { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Due"; // Paid, Partial, Due
    }
}
