using Microsoft.AspNetCore.Identity;

namespace AarMaabd.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Division { get; set; }
        public string? District { get; set; }
        public string? UpazilaThana { get; set; }
        public string? Area { get; set; }
        public string? StreetAddress { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAtUtc { get; set; }
    }
}
