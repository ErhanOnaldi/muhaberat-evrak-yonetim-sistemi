using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string UnitName { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
