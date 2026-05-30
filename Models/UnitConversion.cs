using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class UnitConversion
    {
        [Key]
        public int ConversionID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        [StringLength(20)]
        public string FromUnit { get; set; } // e.g. Carton, Sack

        [Required]
        [StringLength(20)]
        public string ToUnit { get; set; } // e.g. Piece, KG

        [Required]
        public decimal Factor { get; set; } // e.g. 1 Carton = 24 Pieces
    }
}
