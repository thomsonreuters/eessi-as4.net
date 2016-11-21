using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    public class JwtOptions
    {
        public string Issuer { get; } = "ExampleIssuer";
        public string Audience { get; } = "http://localhost:3000";
        public DateTime NotBefore { get; } = DateTime.UtcNow.AddDays(-1);
        private DateTime IssuedAt => DateTime.UtcNow;
        public DateTime Expiration => IssuedAt.AddDays(30);
        public TimeSpan ValidFor => TimeSpan.FromDays(10);
        public string Key { get; } = "fdsqlkfjdsmqlkfjdlkarjezoiparuezop78979";
        public SymmetricSecurityKey SigningKey => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        public SigningCredentials SigningCredentials => new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
    }
}