using muhaberat_evrak_yonetim.Enums;

namespace muhaberat_evrak_yonetim.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDisplayString(this DocumentStatus status)
        {
            return status switch
            {
                DocumentStatus.DRAFT => "Taslak",
                DocumentStatus.SENT => "Gönderildi",
                DocumentStatus.DELIVERED => "Teslim Edildi",
                DocumentStatus.RECEIVED => "Alındı",
                DocumentStatus.CANCELLED => "İptal Edildi",
                _ => status.ToString()
            };
        }

        public static string ToDisplayString(this DeliveryStatus status)
        {
            return status switch
            {
                DeliveryStatus.PREPARING => "Hazırlanıyor",
                DeliveryStatus.SHIPPED => "Kargoya Verildi",
                DeliveryStatus.IN_TRANSIT => "Yolda",
                DeliveryStatus.DELIVERED => "Teslim Edildi",
                DeliveryStatus.RETURNED => "İade Edildi",
                _ => status.ToString()
            };
        }

        public static DocumentStatus ParseDocumentStatus(string statusString)
        {
            return statusString?.ToUpper() switch
            {
                "DRAFT" => DocumentStatus.DRAFT,
                "SENT" => DocumentStatus.SENT,
                "DELIVERED" => DocumentStatus.DELIVERED,
                "RECEIVED" => DocumentStatus.RECEIVED,
                "CANCELLED" => DocumentStatus.CANCELLED,
                _ => DocumentStatus.DRAFT
            };
        }

        public static DeliveryStatus ParseDeliveryStatus(string statusString)
        {
            return statusString?.ToUpper() switch
            {
                "PREPARING" => DeliveryStatus.PREPARING,
                "SHIPPED" => DeliveryStatus.SHIPPED,
                "IN_TRANSIT" => DeliveryStatus.IN_TRANSIT,
                "DELIVERED" => DeliveryStatus.DELIVERED,
                "RETURNED" => DeliveryStatus.RETURNED,
                _ => DeliveryStatus.PREPARING
            };
        }
    }
}