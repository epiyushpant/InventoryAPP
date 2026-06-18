using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Province
    {
        [Key]
        public int ProvinceID { get; set; }

        [Required]
        [StringLength(100)]
        public string ProvinceName { get; set; } = string.Empty;

        [Required]
        public int CountryID { get; set; }
        public Country? Country { get; set; }
    }
}
