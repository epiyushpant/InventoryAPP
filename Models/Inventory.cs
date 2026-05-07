namespace Inventory.Models
{
    public class Inventory
    {
        public int InventoryID { get; set; }
        public int? ProductID { get; set; }
        public int QuantityInStock { get; set; }
        public int? LocationID { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
