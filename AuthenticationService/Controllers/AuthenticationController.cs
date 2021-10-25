using System.Security.Claims;
using AuthenticationService.Business.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly ITokenManager _tokenManager;
        public AuthenticationController(ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        [HttpGet("get-token")]
        public string GetToken(string user)
        {
            return _tokenManager.GenerateToken(user);
        }
    }
}