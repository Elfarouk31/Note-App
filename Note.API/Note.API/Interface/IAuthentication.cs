using Notes.API.Models.Entities;

namespace Notes.API.Interface
{
    public interface IAuthentication
    {
        Task<AuthenticationModel> RegisterAsync(RegisterModel model);
        Task<AuthenticationModel> GetTokenAsync(AuthTokenModel model);
        Task<string> AddUserToRole(UserRole model);
        Task<AuthenticationModel> RefreshTokenAsync(string token);
    }
}
