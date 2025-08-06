using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class DocumentTypeCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = null!;

        [StringLength(20)]
        public string? CategoryCode { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
    }
}