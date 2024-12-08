using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Notes.API.Helper;
using Notes.API.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Notes.API.Models.Entities
{
    public class Authentication : IAuthentication
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtOptions _jwtOptions;
        public Authentication(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager , JwtOptions jwtOptions)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtOptions = jwtOptions;
        }
        public async Task<AuthenticationModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthenticationModel { Message = "Email Exist try another one" };

            if (await _userManager.FindByNameAsync(model.UserName) is not null)
                return new AuthenticationModel { Message = "UserName Exist try another one"};

            var user = new AppUser 
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description},";
                }
                return new AuthenticationModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            var jwtSecurityToken = await CreateJwtToken(user);

            if (jwtSecurityToken is null)
                return new AuthenticationModel { Message = "errors! when get token plz try again" };

            var authenticationModel = new AuthenticationModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                IsAuthenticated = true,
                //ExpirationDate = jwtSecurityToken.ValidTo,
                Roles = new List<string> { "Admin" }
            };

            return authenticationModel;
        }


        public async Task<JwtSecurityToken> CreateJwtToken(AppUser user)
        {
            var userCalim = await _userManager.GetClaimsAsync(user);
            var userRole = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in userRole)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("Id", user.Id)
            }.Union(roleClaims).Union(userCalim);

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding
                .UTF8.GetBytes(_jwtOptions.SigningKey)), SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken
            (
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audiance,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwtOptions.DurationInDays),
                signingCredentials: signingCredentials
            );
            
            return jwtSecurityToken;
        }

        public async Task<AuthenticationModel> GetTokenAsync(AuthTokenModel model)
        {
            var authenticationModel = new AuthenticationModel();

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authenticationModel.Message = "Email or Password not valid";
                return authenticationModel;
            }

            var userRole = await _userManager.GetRolesAsync(user);
            var jwtSecurityToken = await CreateJwtToken(user);

            authenticationModel.IsAuthenticated = true; 
            authenticationModel.UserName = user.UserName;
            authenticationModel.Email = model.Email;
            authenticationModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            //authenticationModel.ExpirationDate = jwtSecurityToken.ValidTo;
            authenticationModel.Roles = userRole.ToList();

            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                authenticationModel.RefreshToken = refreshToken.Token;
                authenticationModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
            }
            else
            {
                var refreshToken = GenrateRefreshToken();
                authenticationModel.RefreshToken = refreshToken.Token;
                authenticationModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);
            }

            return authenticationModel;
        }

        public async Task<string> AddUserToRole(UserRole model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || await _roleManager.RoleExistsAsync(model.Role))
                return "sothing wrong";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "user already Exist";

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "somthing went wrong";        
        }

        public async Task<AuthenticationModel> RefreshTokenAsync(string token)
        {
            var auth = new AuthenticationModel();

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(ur => ur.Token == token));

            var RefrshToken = user.RefreshTokens.Single(t => t.Token == token);

            if ( user is null || !RefrshToken.IsActive)
            {
                auth.IsAuthenticated = false;
                auth.Message = "Invalid token";
                return auth;
            }

            RefrshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenrateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var newToken = await CreateJwtToken(user);

            auth.IsAuthenticated = true;
            auth.Token = new JwtSecurityTokenHandler().WriteToken(newToken);
            auth.RefreshToken = newRefreshToken.Token;
            auth.Roles = _userManager.GetRolesAsync(user).Result.ToList();
            auth.Email = user.Email;
            auth.UserName = user.UserName;

            return auth;
        }

        private RefreshToken GenrateRefreshToken()
        {
            var randomNumber = new byte[32];

            using var genrator = new RNGCryptoServiceProvider();

            genrator.GetBytes(randomNumber);

            RefreshToken refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(10),
                CreatedOn = DateTime.UtcNow
            };

            return refreshToken;
        }
    }
}
