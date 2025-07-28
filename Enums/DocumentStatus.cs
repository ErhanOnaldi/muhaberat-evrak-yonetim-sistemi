namespace muhaberat_evrak_yonetim.Enums
{
    public enum DocumentStatus
    {
        DRAFT = 1,      // Taslak - Evrak hazırlanıyor
        SENT = 2,       // Gönderildi - Evrak kargoya verildi
        DELIVERED = 3,  // Teslim Edildi - Kargo teslim etti
        RECEIVED = 4,   // Alındı - Alıcı tarafından teslim alındı
        CANCELLED = 5   // İptal - Evrak iptal edildi
    }

    public enum DeliveryStatus
    {
        PREPARING = 1,  // Hazırlanıyor - Kargo öncesi hazırlık
        SHIPPED = 2,    // Kargoya Verildi - Kargo şirketine teslim edildi
        IN_TRANSIT = 3, // Yolda - Kargo taşıma sürecinde
        DELIVERED = 4,  // Teslim Edildi - Hedefe ulaştı
        RETURNED = 5    // İade Edildi - Geri gönderildi
    }
}
