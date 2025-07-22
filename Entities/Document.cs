using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentNumber { get; set; } = null!;

        public int? DocumentTypeId { get; set; }
        public DocumentType? DocumentType { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        // Sender and Receiver Information (BIDIRECTIONAL)
        public int SenderUserId { get; set; }
        public User SenderUser { get; set; } = null!;

        public int? SenderDepartmentId { get; set; }
        public Department? SenderDepartment { get; set; }

        public int? ReceiverUserId { get; set; }
        public User? ReceiverUser { get; set; }

        public int? ReceiverDepartmentId { get; set; }
        public Department? ReceiverDepartment { get; set; }

        // Customer information (optional)
        [StringLength(100)]
        public string? CustomerId { get; set; }

        [StringLength(200)]
        public string? CustomerName { get; set; }

        // Upload information
        public int CreatedBy { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        // Physical document information
        [StringLength(50)]
        public string? PhysicalDocumentType { get; set; } // ORIGINAL, COPY, NOTARIZED

        [StringLength(50)]
        public string? PackageType { get; set; } // ENVELOPE, SMALL_PACKAGE, LARGE_PACKAGE, SPECIAL

        // Address information
        public string ShippingAddress { get; set; } = null!;
        public string DeliveryAddress { get; set; } = null!;

        // Cargo information (manual update)
        [StringLength(100)]
        public string? CargoCompany { get; set; }

        [StringLength(50)]
        public string? CargoTrackingNumber { get; set; }

        public DateTime? ShippingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }

        [StringLength(20)]
        public string DeliveryStatus { get; set; } = "PREPARING"; // PREPARING, SHIPPED, IN_TRANSIT, DELIVERED, RETURNED

        [StringLength(100)]
        public string? ReceivedBy { get; set; }

        public string? DeliveryNotes { get; set; }

        // Status information
        public bool IsActive { get; set; } = true;

        [StringLength(20)]
        public string Status { get; set; } = "DRAFT"; // DRAFT, SENT, DELIVERED, RECEIVED, CANCELLED

        // Approval process
        public int? ReviewedBy { get; set; }
        public User? ReviewedByUser { get; set; }

        public DateTime? ReviewDate { get; set; }
        public string? ReviewNotes { get; set; }

        // File information
        public string? FilePath { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<DocumentHistory> DocumentHistories { get; set; } = new List<DocumentHistory>();
        public ICollection<CargoTrackingLog> CargoTrackingLogs { get; set; } = new List<CargoTrackingLog>();
    }
}
