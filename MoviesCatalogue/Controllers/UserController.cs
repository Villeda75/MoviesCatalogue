using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MoviesCatalogue.Classes;
using MoviesCatalogue.Context;
using MoviesCatalogue.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
                new Claim("Username", user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
            var sigIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                jwt.Issuer,
                jwt.Audience,
                claims,
                //expires: DateTime.Now.AddMinutes(15),
                expires: DateTime.Now.AddHours(5),
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

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            string errorMessage = "Failed to register user.";

            try
            {   
                if (user is null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        status = 400,
                        message = errorMessage,
                        error = "Object null"
                        
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        status = 400,
                        message = errorMessage,
                        error = "Missing required fields: Name, Email, Password and Role"
                        
                    });
                }

                if (!IsValidEmail(user.Email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        status = 400,
                        message = errorMessage,
                        error = "Invalid email"
                    });
                }

               
                if (IsTheUserExist(user.Email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        status = 400,
                        message = errorMessage,
                        error = "User already exists"
                    });
                }
                    

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    status = 201,
                    message = "Successfully registered user.",
                    data = user,
                    error = ""
                });
            }
            catch (Exception error)
            {
                return BadRequest(new
                {
                    success = false,
                    status = 400,
                    message = errorMessage,
                    error = error.Message
                });
            }
           
        }

        [NonAction]
        public bool IsTheUserExist(string Email)
        {
            return _context.Users.Where(x => x.Email.Equals(Email)).Any();
        }

        [NonAction]
        public bool IsValidEmail(string Email)
        {
            return new EmailAddressAttribute().IsValid(Email);
        }
    }
}
