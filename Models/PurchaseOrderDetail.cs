namespace Inventory.Models
{
    public class PurchaseOrderDetail
    {
        public int PODetailID { get; set; }
        public int? PurchaseOrderID { get; set; }
        public int? ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}
