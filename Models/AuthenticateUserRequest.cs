using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Models
{
    public class AuthenticateUserRequest
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
