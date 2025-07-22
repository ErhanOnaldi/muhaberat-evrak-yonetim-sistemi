using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        // A user can belong to one unit
        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Document> SentDocuments { get; set; } = new List<Document>();
        public ICollection<Document> ReceivedDocuments { get; set; } = new List<Document>();
        public ICollection<Document> CreatedDocuments { get; set; } = new List<Document>();
        public ICollection<Document> ReviewedDocuments { get; set; } = new List<Document>();
        public ICollection<DocumentHistory> DocumentHistories { get; set; } = new List<DocumentHistory>();
    }
}