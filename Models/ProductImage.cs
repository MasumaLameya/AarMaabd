using System.ComponentModel.DataAnnotations;

namespace AarMaabd.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required, MaxLength(300)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? AltText { get; set; }

        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
    }
}
