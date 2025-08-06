using muhaberat_evrak_yonetim.Models;

namespace muhaberat_evrak_yonetim.Services
{
    public interface IAuthentication
    {
        Task<AuthenticateUserResult> AuthenticateUser(AuthenticateUserRequest userSignInRequest);
    }
}
