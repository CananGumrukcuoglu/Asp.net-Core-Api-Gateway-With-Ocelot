using AuthenticationService.Business.BusinessObjects.Responses;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService.Business.Interfaces
{
    public interface ITokenManager
    {
        string GenerateToken(string loginUser);
        Task<IPrincipal> AuthenticateJwtToken(string token);
        bool ValidateAppToken(string token, out string username);
        bool ValidateAppToken(string token);    

    }
}
