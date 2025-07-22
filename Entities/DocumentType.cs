using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class DocumentType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TypeName { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string TypeCode { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsUrgent { get; set; } = false;
        public bool RequiresSignature { get; set; } = false;

        public string? PackageRequirements { get; set; }

        public int EstimatedDeliveryDays { get; set; } = 3;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<DocumentPermission> DocumentPermissions { get; set; } = new List<DocumentPermission>();
    }
}
