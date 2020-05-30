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
using KansonNetCoreBackend.Models;
using KansonNetCoreBackend.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using KansonNetcoreBackend.Models;

namespace KansonNetCoreBackend.Controllers
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

        [AllowAnonymous]
        [HttpGet("googleLogin/{id}")]
        public IActionResult LoginGoogle(string id)
        {
            Debug.WriteLine("Google Token :" + id);
            var user = ValidateGoogleToken(_context, id, _appSettings);

            if (user == null)
                return BadRequest(new { message = "Can't login via Google!" });

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

        private const string GoogleApiTokenInfoUrl = "https://oauth2.googleapis.com/tokeninfo?id_token={0}";

        public static UsersDTO ValidateGoogleToken(TrelloKeepContext _context, string providerToken, AppSettings _appSettings)
        {

            var httpClient = new HttpClient();

            var requestUri = new Uri(string.Format(GoogleApiTokenInfoUrl, providerToken));

            HttpResponseMessage httpResponseMessage;
            try
            {
                httpResponseMessage = httpClient.GetAsync(requestUri).Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message.ToString());
                return null;
            }

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                Debug.WriteLine("httpResponseMessage.StatusCode != HttpStatusCode.OK");
                return null;
            }

            var response = httpResponseMessage.Content.ReadAsStringAsync().Result;
            var googleApiTokenInfo = JsonConvert.DeserializeObject<GoogleApiTokenInfo>(response);

            var user = _context.Users.ToList().FirstOrDefault(x => x.Id == googleApiTokenInfo.sub);
            if (user == null)
            {
                user = new Users
                {
                    Id = googleApiTokenInfo.sub,
                    FirstName = googleApiTokenInfo.given_name,
                    LastName = googleApiTokenInfo.family_name,
                    Username = googleApiTokenInfo.name,
                    Password = "",
                    Token = GenerateNewToken(googleApiTokenInfo.sub, _appSettings.Secret)
            };
                _context.Users.Add(user);

                UsersController.InitializeNewUser(_context, user);
            }
            else
            {
                // authentication successful so generate jwt token
                user.Token = GenerateNewToken(user.Id, _appSettings.Secret);
            }


            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (UsersController.UsersExists(_context, user.Id))
                {
                    Debug.WriteLine("USER ALREADY EXIST!");
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return UsersController.ItemToDTO(user);
        }
    }
}
