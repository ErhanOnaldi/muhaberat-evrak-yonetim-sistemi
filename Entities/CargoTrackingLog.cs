using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class CargoTrackingLog
    {
        [Key]
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public Document Document { get; set; } = null!;

        [StringLength(20)]
        public string? OldStatus { get; set; }

        [StringLength(20)]
        public string? NewStatus { get; set; }

        public DateTime StatusChangeDate { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public int? UpdatedBy { get; set; }
        public User? UpdatedByUser { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
