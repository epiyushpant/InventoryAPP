namespace Inventory.Models
{
    public class Sale
    {
        public int SaleID { get; set; }
        public DateTime SaleDate { get; set; }
        public int? CustomerID { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
    }
}
