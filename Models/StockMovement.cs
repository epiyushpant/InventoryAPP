namespace Inventory.Models
{
    public class StockMovement
    {
        public int MovementID { get; set; }
        public int? ProductID { get; set; }
        public string? MovementType { get; set; }
        public int? QuantityChange { get; set; }
        public DateTime? MovementDate { get; set; }
        public string? Reference { get; set; }
    }
}
