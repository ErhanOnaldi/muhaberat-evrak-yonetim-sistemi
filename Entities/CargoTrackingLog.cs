using System;

namespace muhaberat_evrak_yonetim.Entities
{
    public class CargoTrackingLog
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public Document Document { get; set; }

        public string CargoTrackingNumber { get; set; } = null!;
        public string CargoCompanyName { get; set; } = null!;
        public DateTime SentDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string Status { get; set; } = null!;
    }
}
