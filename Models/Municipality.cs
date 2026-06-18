using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Municipality
    {
        [Key]
        public int MunicipalityID { get; set; }

        [Required]
        [StringLength(150)]
        public string MunicipalityName { get; set; } = string.Empty;

        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // e.g. Municipality, Rural Municipality, Metropolitan City

        [Required]
        public int DistrictID { get; set; }
        public District? District { get; set; }
    }
}
