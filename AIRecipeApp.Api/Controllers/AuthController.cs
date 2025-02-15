using AIRecipeApp.Api.Context;
using AIRecipeApp.Api.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AIRecipeApp.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(MongoDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 📌 Kullanıcı Kayıt
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var existingUser = await _context.Users.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
            if (existingUser != null)
                return BadRequest("Bu kullanıcı adı zaten alınmış.");

            await _context.Users.InsertOneAsync(user);
            return Ok(new { message = "Kullanıcı başarıyla kaydedildi!" });
        }

        // 📌 Kullanıcı Girişi (JWT Token Oluşturma)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var existingUser = await _context.Users.Find(u => u.Username == user.Username && u.Password == user.Password).FirstOrDefaultAsync();
            if (existingUser == null)
                return Unauthorized("Kullanıcı adı veya şifre hatalı!");

            var token = GenerateJwtToken(existingUser);
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var keyString = _config["Jwt:Key"];

            if (string.IsNullOrEmpty(keyString) || keyString.Length < 32)
            {
                throw new ArgumentException("JWT key must be at least 32 characters long.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
        };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
