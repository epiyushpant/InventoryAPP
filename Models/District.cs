using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class District
    {
        [Key]
        public int DistrictID { get; set; }

        [Required]
        [StringLength(100)]
        public string DistrictName { get; set; } = string.Empty;

        [Required]
        public int ProvinceID { get; set; }
        public Province? Province { get; set; }
    }
}
