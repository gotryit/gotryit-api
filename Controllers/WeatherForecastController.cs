using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using gotryit_api.Entities;
using gotryit_api.Repositories;
using System.Text;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace gotryit_api.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Route("api/weather")]
    public class WeatherForecastController : Controller
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly GoTryItContext db;

        public WeatherForecastController(GoTryItContext db)
        {
            this.db = db;
        }

        [HttpGet]        
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]        
        public IActionResult Authenticate([FromBody] AuthenticationData authenticationData, [FromQuery] bool toCookie)
        {
            var user = db.User.Single(u => u.Name == authenticationData.UserName);

            var keyDerivation = new Rfc2898DeriveBytes(Encoding.ASCII.GetBytes(authenticationData.Password), 
                                                       Convert.FromBase64String(user.PasswordSalt), 
                                                       10000);

            var computedHash = keyDerivation.GetBytes(32);

            string userToken = string.Empty;

            if (user.PasswordHash == Convert.ToBase64String(computedHash)) 
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new byte[32];
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] 
                                {
                                    new Claim(ClaimTypes.Name, authenticationData.UserName),
                                    new Claim(ClaimTypes.Role, "Administrator")
                                }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                
                var token = tokenHandler.CreateToken(tokenDescriptor);
                
                userToken = tokenHandler.WriteToken(token);
            }

            if(toCookie)
            {
                HttpContext.Response.Cookies.Append("UserAuthentication", userToken, new Microsoft.AspNetCore.Http.CookieOptions(){
                    Expires = DateTime.UtcNow.AddDays(7),
                    HttpOnly = true,
                    Secure = true
                });

                return Ok();
            }

            return Ok(new AuthenticatedUser{
                Token = userToken,
                ExpireDate = DateTime.UtcNow.AddDays(7)
            });
        }


    }
}
