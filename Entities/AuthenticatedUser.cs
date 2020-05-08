using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace gotryit_api.Entities
{
    public class AuthenticatedUser
    {
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
    }

    public class AuthenticationData
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class Authenticator
    {
        private readonly IConfiguration configuration;

        public Authenticator(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string GetToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, "user")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}