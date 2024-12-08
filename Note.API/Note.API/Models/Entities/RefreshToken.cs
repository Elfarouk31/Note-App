using Microsoft.EntityFrameworkCore;

namespace Notes.API.Models.Entities
{
    [Owned]
    public class RefreshToken
    {
        public string Token { get; set; }
        public bool IsActive => RevokedOn == null && !IsExpired;
        public bool IsExpired => ExpiresOn <= DateTime.UtcNow;
        public DateTime CreatedOn { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime? RevokedOn { get; set; }
    }
}
