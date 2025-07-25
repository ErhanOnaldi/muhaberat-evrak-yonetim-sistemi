using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string DepartmentCode { get; set; } = null!;

        public string? Description { get; set; }

        public bool HasFullAccess { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Document> SentDocuments { get; set; } = new List<Document>();
        public ICollection<Document> ReceivedDocuments { get; set; } = new List<Document>();
        public ICollection<DocumentPermission> DocumentPermissions { get; set; } = new List<DocumentPermission>();
    }
}
