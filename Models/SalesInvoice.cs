using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class SalesInvoice
    {
        [Key]
        public int InvoiceID { get; set; }

        [Required]
        public int SaleID { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public decimal TaxAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal NonTaxableAmount { get; set; }

        public decimal GrandTotal { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Due"; // Paid, Partial, Due
    }
}
