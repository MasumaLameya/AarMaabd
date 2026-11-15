using System.ComponentModel.DataAnnotations;

namespace AarMaabd.Models
{
    public class SubCategory
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Slug { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
