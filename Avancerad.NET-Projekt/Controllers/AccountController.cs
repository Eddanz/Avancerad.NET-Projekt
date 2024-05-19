using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Avancerad.NET_Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            IdentityUser User = await _userManager.FindByEmailAsync(email);
            if (User == null)
                return NotFound();

            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.CheckPasswordSignInAsync(User!, password, false);
            if (!result.Succeeded)
                return BadRequest();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: await _userManager.GetClaimsAsync(User),
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );
            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }

        //[HttpPost]
        //[Route("/create/admin")]
        //public async Task<IActionResult> CreateAdmin(string email, string password)
        //{
        //    IdentityUser User = await _userManager.FindByEmailAsync(email);
        //    if (User != null)
        //        return BadRequest(false);

        //    IdentityUser user = new()
        //    {
        //        UserName = email,
        //        PasswordHash = password,
        //        Email = email,
        //    };

        //    IdentityResult result = await _userManager.CreateAsync(user, password);

        //    if (!result.Succeeded)
        //        return BadRequest(false);

        //    Claim[] userClaims =
        //        [
        //            new Claim(ClaimTypes.Email, email),
        //            new Claim(ClaimTypes.Role, "admin")
        //        ];
        //    await _userManager.AddClaimsAsync(user, userClaims);

        //    return Ok(true);
        //}
    }
}
