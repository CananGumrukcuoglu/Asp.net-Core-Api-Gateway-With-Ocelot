using AuthenticationService.Business.BusinessObjects.Responses;
using AuthenticationService.Business.Interfaces;
using AuthenticationService.Business.Settings;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.Common.Constants;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService.Business.Implementations
{
    public class TokenManager : ITokenManager
    {
        private readonly ILogger<TokenManager> _logger;
        private readonly TokenConfig _tokenConfig;

        public TokenManager(ILogger<TokenManager> logger,
            IOptions<TokenConfig> tokenConfigOptions)
        {
            _logger = logger;
            _tokenConfig = tokenConfigOptions.Value;
        }

        public string GenerateToken(string loginUser)
        {
            _logger.LogInformation("Generating user token!");
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.Secret));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                    {
                            new Claim(JwtClaimNames.UserId, loginUser.ToString())
                    }),
                Expires = DateTime.UtcNow.AddMinutes(_tokenConfig.AccessExpiration),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256),
                Audience = _tokenConfig.Audience,
                Issuer = _tokenConfig.Issuer
            };
            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);

            _logger.LogInformation("User token generated!");
            return token;
        }

        public string GenerateAppToken()
        {
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.AppSecret));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(_tokenConfig.AccessExpiration),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256),
                Audience = _tokenConfig.Audience,
                Issuer = _tokenConfig.Issuer
            };
            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);
            return token;
        }

        public Task<IPrincipal> AuthenticateJwtToken(string token)
        {
            if (!ValidateToken(token, out var username)) return Task.FromResult<IPrincipal>(null);
            // based on username to get more information from database in order to build local identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
                // Add more claims if needed: Roles, ...
            };

            var identity = new ClaimsIdentity(claims, "Jwt");
            IPrincipal user = new ClaimsPrincipal(identity);

            return Task.FromResult(user);
        }

        private ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var symmetricKey = Encoding.UTF8.GetBytes(_tokenConfig.Secret);

                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _tokenConfig.Issuer,
                    ValidAudience = _tokenConfig.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        private ClaimsPrincipal GetAppPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var symmetricKey = Encoding.UTF8.GetBytes(_tokenConfig.AppSecret);

                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _tokenConfig.Issuer,
                    ValidAudience = _tokenConfig.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey),
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        private bool ValidateToken(string token, out string username)
        {
            username = null;
            var simplePrinciple = this.GetPrincipal(token);
            var identity = simplePrinciple.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var usernameClaim = identity.FindFirst(JwtClaimNames.UserId);
            username = usernameClaim?.Value;

            if (string.IsNullOrEmpty(username))
                return false;
            return true;
        }

        public bool ValidateAppToken(string token)
        {
            var simplePrinciple = this.GetAppPrincipal(token);
            var identity = simplePrinciple.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            return true;
        }
        public bool ValidateAppToken(string token, out string username)
        {
            username = null;
            var simplePrinciple = this.GetAppPrincipal(token);
            var identity = simplePrinciple.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var usernameClaim = identity.FindFirst(_tokenConfig.ISalesClaimName);
            username = usernameClaim?.Value;

            if (string.IsNullOrEmpty(username))
                return false;
            return true;
        }
    }
}
