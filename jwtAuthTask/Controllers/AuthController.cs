using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using userLogin;
using userRegistration;

namespace authcontrollers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static Register register = new Register();
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Register>>userRegister(Login login)
        {
            CreatePasswordHash(login.password, out byte[] passwordHash, out byte[] passwordSalt);

            register.email = login.email;
            register.passwordHash = passwordHash;
            register.passwordSalt = passwordSalt;
            
            return Ok(register);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> userLogin(Login login)
        {
            if(register.email != login.email)
            {
                return BadRequest("User not found.");
            }

            if(!verifyPassword(login.password, register.passwordHash, register.passwordSalt))
            {
                return BadRequest("Wrong Password");
            }

            string token = createToken(register);

            return Ok(token);
        }

        private string createToken(Register registers)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, registers.email),
                new Claim(ClaimTypes.Role, "admin")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("TokenValue:Token").Value));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool verifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512(passwordSalt))
            {
                var ComputeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return ComputeHash.SequenceEqual(passwordHash);
            }
        }

    }
}