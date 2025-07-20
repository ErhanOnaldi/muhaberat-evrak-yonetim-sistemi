using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime CreationDate { get; set; }

        public string FilePath { get; set; }

        // Relationships
        public int SenderUserId { get; set; }
        public User SenderUser { get; set; }

        public int ReceivingUnitId { get; set; }
        public Unit ReceivingUnit { get; set; }
    }
}
