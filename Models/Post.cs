using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Post
    {
        [Key]
        public int PostID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string? AuthorId { get; set; }
        public virtual ApplicationUser? Author { get; set; }
    }
}
