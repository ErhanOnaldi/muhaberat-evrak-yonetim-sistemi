namespace muhaberat_evrak_yonetim.Models
{
    public static class LdapConstraints
    {
        public static string RequestErrorMessage { get; set; } = "Bağlantı kurulamadı.";
        public static string InvalidUsernamePasswordMessage { get; set; } = "Kullanıcı adı / şifre yanlış";
    }
}
