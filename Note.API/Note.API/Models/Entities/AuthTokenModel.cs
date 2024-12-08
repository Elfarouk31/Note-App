using System.ComponentModel.DataAnnotations;

namespace Notes.API.Models.Entities
{
    public class AuthTokenModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
