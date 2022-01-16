namespace Overwurd.Web.Options
{
    public class JwtConfigurationJson
    {
        public string SigningKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int AccessTokenExpirationInMinutes { get; set; }

        public int RefreshTokenExpirationInDays { get; set; }
    }
}