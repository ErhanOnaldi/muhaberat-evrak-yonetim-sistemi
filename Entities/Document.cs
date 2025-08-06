using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string DocumentNumber { get; set; } = null!;

        [Required(ErrorMessage = "Evrak türü seçimi zorunludur.")]
        public int? DocumentTypeId { get; set; }
        public DocumentType? DocumentType { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Gönderen kullanıcı seçimi zorunludur.")]
        public int SenderUserId { get; set; }
        public User? SenderUser { get; set; }

        [Required(ErrorMessage = "Gönderen departman seçimi zorunludur.")]
        public int SenderDepartmentId { get; set; }
        public Department? SenderDepartment { get; set; }

        public int? ReceiverUserId { get; set; }
        public User? ReceiverUser { get; set; }

        public int? ReceiverDepartmentId { get; set; }
        public Department? ReceiverDepartment { get; set; }

        [StringLength(100)]
        public string? CustomerId { get; set; }

        [StringLength(200)]
        public string? CustomerName { get; set; }

        public int CreatedBy { get; set; }
        public User? CreatedByUser { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required(ErrorMessage = "Fiziksel evrak türü seçimi zorunludur.")]
        [StringLength(50)]
        public string PhysicalDocumentType { get; set; } = null!;

        public int? CargoId { get; set; }
        public Cargo? Cargo { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(20)]
        public string Status { get; set; } = "DRAFT"; 

        public int? ReviewedBy { get; set; }
        public User? ReviewedByUser { get; set; }

        public DateTime? ReviewDate { get; set; }
        public string? ReviewNotes { get; set; }

        public string? FilePath { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<DocumentHistory> DocumentHistories { get; set; } = new List<DocumentHistory>();
        public ICollection<CargoTrackingLog> CargoTrackingLogs { get; set; } = new List<CargoTrackingLog>();
    }
}
