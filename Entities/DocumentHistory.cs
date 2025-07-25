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
        public string ActionType { get; set; } = null!; 

        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime ActionDate { get; set; }

        public string? OldValues { get; set; } 
        public string? NewValues { get; set; } 

        public string? Notes { get; set; }

        public string? IpAddress { get; set; }
    }
}
