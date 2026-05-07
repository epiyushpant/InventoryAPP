namespace Inventory.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int? CategoryID { get; set; }
        public int? SupplierID { get; set; }
        public string? SKU { get; set; }
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int? ReorderLevel { get; set; }
        public bool? IsActive { get; set; }
    }
}
