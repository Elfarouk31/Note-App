using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Notes.API.Interface;
using Notes.API.Models.Entities;

namespace Notes.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthentication _authentication;
        public AuthController(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authentication.RegisterAsync(registerModel);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(AuthTokenModel authTokenModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authentication.GetTokenAsync(authTokenModel);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            if (!string.IsNullOrEmpty(result.RefreshToken))
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }

        [HttpPost("AddToRole")]
        public async Task<IActionResult> AddToRole(UserRole userRole)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authentication.AddUserToRole(userRole);

            return Ok(userRole);
        }

        private void SetRefreshTokenInCookie(string token, DateTime expires)
        {
            var cookiesOptions = new CookieOptions 
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime()
            };

            Response.Cookies.Append("refreshToken", token, cookiesOptions);
        }
    }
}
