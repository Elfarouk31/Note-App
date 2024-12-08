using Notes.API.Interface;
using System.Text.Json.Serialization;

namespace Notes.API.Models.Entities
{
    public class AuthenticationModel 
    {
        public string UserName { get; set; }    
        public string Email { get; set; }
        public string Token{ get; set; }
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public List<string> Roles { get; set; }
        //public DateTime ExpirationDate { get; set; }

        [JsonIgnore]
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
