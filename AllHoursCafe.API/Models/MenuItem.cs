using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AllHoursCafe.API.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsVegetarian { get; set; }

        public bool IsVegan { get; set; }

        public bool IsGlutenFree { get; set; }

        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }

        // Use JsonIgnore to prevent circular references when serializing to JSON
        // We'll still include the CategoryId which is enough for the client
        [JsonIgnore]
        public Category Category { get; set; }

        [StringLength(50)]
        public string? SpicyLevel { get; set; }

        public int? PrepTimeMinutes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Calories { get; set; }
    }
}