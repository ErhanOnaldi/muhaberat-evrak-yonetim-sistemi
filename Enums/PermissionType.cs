namespace muhaberat_evrak_yonetim.Enums
{
    public enum PermissionType
    {
        Read = 1,       // Okuma
        Write = 2,      // Düzenleme (Taslak halindeyken)
        Approve = 3,    // Onaylama
        Forward = 4,    // Havale Etme
        Comment = 5     // Yorum Yapma
    }
}
