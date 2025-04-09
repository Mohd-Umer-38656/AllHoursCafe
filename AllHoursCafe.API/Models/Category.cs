using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AllHoursCafe.API.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore] // Prevent circular references in JSON serialization
        public ICollection<MenuItem>? MenuItems { get; set; }
    }
}