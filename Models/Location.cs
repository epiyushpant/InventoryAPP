namespace Inventory.Models
{
    public class Location
    {
        public int LocationID { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; } // e.g. Warehouse, Storefront
    }
}
