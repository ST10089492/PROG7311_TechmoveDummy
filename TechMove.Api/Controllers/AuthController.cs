using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TechMove.Api.Dtos;

namespace TechMove.Api.Controllers
{
    // hands out a jwt when the seeded admin logs in, the mvc frontend keeps that token
    // and sends it on the calls that change data (The IIE, 2026)
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            // one seeded account kept in config, this is enough for the prototype
            var username = _config["Auth:Username"];
            var password = _config["Auth:Password"];

            if (dto.Username != username || dto.Password != password)
                return Unauthorized("Wrong username or password.");

            var token = BuildToken(dto.Username);
            return Ok(new { token });
        }

        private string BuildToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
