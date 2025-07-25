using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class DocumentPermission
    {
        [Key]
        public int Id { get; set; }

        public int? DocumentTypeId { get; set; }
        public DocumentType? DocumentType { get; set; }

        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public bool CanView { get; set; } = false;
        public bool CanUpload { get; set; } = false;
        public bool CanDownload { get; set; } = false;
        public bool CanReview { get; set; } = false;
        public bool CanApprove { get; set; } = false;

        public DateTime CreatedAt { get; set; }
    }
}
