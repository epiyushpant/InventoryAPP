namespace Inventory.Models
{
    public class PurchaseOrder
    {
        public int PurchaseOrderID { get; set; }
        public int? SupplierID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public string? Status { get; set; }
    }
}
