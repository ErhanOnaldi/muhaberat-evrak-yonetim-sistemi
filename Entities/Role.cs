using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string RoleCode { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<DocumentPermission> DocumentPermissions { get; set; } = new List<DocumentPermission>();
    }
}
