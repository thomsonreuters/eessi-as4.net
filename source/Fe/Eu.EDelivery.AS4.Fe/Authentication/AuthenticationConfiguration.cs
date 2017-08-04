namespace Eu.EDelivery.AS4.Fe.Authentication
{
    public class AuthenticationConfiguration
    {
        public string ConnectionString { get; set; }
        public string Provider { get; set; }
        public Jwt JwtOptions { get; set; }
    }

    public class Jwt
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ValidFor { get; set; }
        public string Key { get; set; }
    }
}