using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class Cargo
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string? CargoTrackingNumber { get; set; }

        [StringLength(100)]
        public string? CargoCompany { get; set; }

        [Required(ErrorMessage = "Gönderim adresi zorunludur.")]
        public string ShippingAddress { get; set; } = null!;
        
        [Required(ErrorMessage = "Teslimat adresi zorunludur.")]
        public string DeliveryAddress { get; set; } = null!;

        [Required(ErrorMessage = "Paket türü seçimi zorunludur.")]
        [StringLength(50)]
        public string PackageType { get; set; } = null!;

        public DateTime? ShippingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }

        [StringLength(20)]
        public string DeliveryStatus { get; set; } = "PREPARING"; 

        [StringLength(100)]
        public string? ReceivedBy { get; set; }

        public string? DeliveryNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<CargoTrackingLog> CargoTrackingLogs { get; set; } = new List<CargoTrackingLog>();
    }
}