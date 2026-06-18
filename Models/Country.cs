using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Country
    {
        [Key]
        public int CountryID { get; set; }

        [Required]
        [StringLength(100)]
        public string CountryName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string IsoCode { get; set; } = string.Empty;
    }
}
