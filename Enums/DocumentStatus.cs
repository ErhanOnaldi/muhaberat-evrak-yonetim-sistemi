namespace muhaberat_evrak_yonetim.Enums
{
    public enum DocumentStatus
    {
        Draft = 1,          // Taslak
        Sent = 2,           // Gönderildi
        Read = 3,           // Okundu
        InProgress = 4,     // İşlemde
        Approved = 5,       // Onaylandı
        Rejected = 6,       // Reddedildi
        Archived = 7,       // Arşivlendi
        Canceled = 8        // İptal Edildi
    }
}
