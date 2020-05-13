using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using KansonBackendNetCore.Models;
using KansonBackendNetCore.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace KansonBackendNetCore.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TrelloKeepContext _context;
        private readonly AppSettings _appSettings;

        public AuthController(TrelloKeepContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate([FromBody]AuthModel model)
        {
            var user = Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        public UsersDTO Authenticate(string username, string password)
        {
            var user = _context.Users.ToList().FirstOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            user.Token = GenerateNewToken(user.Id, _appSettings.Secret);

            _context.Entry(user).State = EntityState.Modified;
            Debug.WriteLine(user.Token);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }

            return UsersController.ItemToDTO(user);
        }

        public static string GenerateNewToken(string id, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, id)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
