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
    using MoviesCatalogue.Classes.Wrappers;

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
            string message = "Failed to authenticate user.";

            try
            {
                if (credentials is null)
                {
                    return BadRequest(new AuthenticationResponse<dynamic>(message, "empty email or password", "", ""));
                }

                var user = _context.Users.Where(x => x.Email == credentials.Email && x.Password == credentials.Password).FirstOrDefault();

                if (user == null)
                {
                    return BadRequest(new AuthenticationResponse<dynamic>(message, "invalid credentials", "", ""));
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

                return Ok(new AuthenticationResponse<dynamic>(
                        "successfully authenticated user.",
                        user.Name,
                        new JwtSecurityTokenHandler().WriteToken(token)
                    ));
                
            }
            catch (Exception error)
            {
                return BadRequest(new AuthenticationResponse<dynamic>(message, error.Message, "", ""));
            }
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            string message = "Failed to register user.";

            try
            {   
                if (user is null)
                {
                    return BadRequest(new Response<User>(message, "Object null", user));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new Response<User>(message, "Missing required fields: Name, Email, Password and Role", user)); 
                }

                if (!IsValidEmail(user.Email))
                {
                    return BadRequest(new Response<User>(message, "Invalid email", user));
                }

               
                if (IsTheUserExist(user.Email))
                {
                    return BadRequest(new Response<User>(message, "User already exists", user));
                }
                    

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new Response<User>("Successfully registered user.", user));
            }
            catch (Exception error)
            {
                return BadRequest(new Response<User>(message, error.Message, user));
            }
           
        }

        private bool IsTheUserExist(string Email)
        {
            return _context.Users.Where(x => x.Email.Equals(Email)).Any();
        }

        private static bool IsValidEmail(string Email)
        {
            return new EmailAddressAttribute().IsValid(Email);
        }
    }
    }
