using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using muhaberat_evrak_yonetim.Models;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Net;

namespace muhaberat_evrak_yonetim.Services
{
    public class Authentication : IAuthentication
    {
        private readonly LdapOption _ldapOption;

        public Authentication(IOptions<LdapOption> ldapOption)
        {
            _ldapOption = ldapOption.Value;
        }


        public async Task<AuthenticateUserResult>  AuthenticateUser(AuthenticateUserRequest userSignInRequest)
        {
            AuthenticateUserResult authenticateUserResult = new AuthenticateUserResult();
            LdapDirectoryIdentifier ldi = new LdapDirectoryIdentifier(_ldapOption.LdapServer, int.Parse(_ldapOption.LdapPortNumber));
            LdapConnection ldapConnection = new LdapConnection(ldi);

            ldapConnection.AuthType = AuthType.Basic;
            ldapConnection.SessionOptions.ProtocolVersion = 3;
            try
            {
                NetworkCredential nc = new NetworkCredential(_ldapOption.LdapUser, _ldapOption.LdapPassword);
                ldapConnection.Bind(nc);
                using (var context = new PrincipalContext(ContextType.Domain, _ldapOption.LdapServer, _ldapOption.LdapDomain))
                {
                    bool isAuthenticated = context.ValidateCredentials(userSignInRequest.UserName, userSignInRequest.Password);

                    if (isAuthenticated)
                    {
                        authenticateUserResult.IsSuccess = true;
                        authenticateUserResult.Data = true;
                    }
                    else
                    {
                        authenticateUserResult.IsSuccess = false;
                        authenticateUserResult.Data = false;
                        authenticateUserResult.Message = LdapConstraints.InvalidUsernamePasswordMessage;
                    }
                    return authenticateUserResult;
                }
            }
            catch (Exception ex)
            {
                authenticateUserResult.IsSuccess = false;
                authenticateUserResult.Message = LdapConstraints.RequestErrorMessage;
                return authenticateUserResult;
            }
        }
    }
}
