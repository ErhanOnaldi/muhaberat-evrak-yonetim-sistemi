using System;

namespace muhaberat_evrak_yonetim.Entities
{
    public class DocumentHistory
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public Document Document { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Action { get; set; } = null!;
        public DateTime ActionDate { get; set; }
        public string? Description { get; set; } 
    }
}
