namespace Notes.API.Helper
{
    public class JwtOptions
    {
        public string Issuer { get; set; }
        public string Audiance { get; set; }
        public string SigningKey { get; set; }
        public double DurationInDays { get; set; }
    }
}
