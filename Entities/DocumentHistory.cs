using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class DocumentHistory
    {
        [Key]
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public Document Document { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } = null!; // UPLOADED, VIEWED, DOWNLOADED, APPROVED, REJECTED

        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime ActionDate { get; set; }

        public string? OldValues { get; set; } // JSON string
        public string? NewValues { get; set; } // JSON string

        public string? Notes { get; set; }

        public string? IpAddress { get; set; }
    }
}
