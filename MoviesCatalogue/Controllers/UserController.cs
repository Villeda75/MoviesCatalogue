using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesCatalogue.Classes;
using MoviesCatalogue.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MoviesCatalogue.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public IConfiguration _configuration;

        public UserController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public dynamic Login(Credentials credentials)
        {
            if (credentials is null)
            {
                return new
                {
                    success = false,
                    message = "empty email or password",
                    user = "",
                    token = ""
                };
            }

            var user = _context.Users.Where(x => x.Email == credentials.Email && x.Password == credentials.Password).FirstOrDefault();

            if (user == null) {
                return new
                {
                    success = false,
                    message = "invalid credentials",
                    user = "",
                    token = ""
                };
            }

            var jwt = _configuration.GetSection("Jwt").Get<Jwt>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("Id", user.Id.ToString()),
                new Claim("Username", user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
            var sigIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                jwt.Issuer,
                jwt.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: sigIn
                );

            return new
            {
                success = true,
                message = "successfully authenticated user",
                user = user.Name,
                token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }
    }
}
