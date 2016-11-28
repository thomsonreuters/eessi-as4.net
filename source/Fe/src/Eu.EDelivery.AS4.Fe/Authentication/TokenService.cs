using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    public class TokenService : ITokenService
    {
        private readonly IOptions<JwtOptions> jwtOptions;

        public TokenService(IOptions<JwtOptions> jwtOptions)
        {
            this.jwtOptions = jwtOptions;
        }

        public string GenerateToken()
        {
            var options = jwtOptions.Value;

            var jwt = new JwtSecurityToken(
                options.Issuer,
                options.Audience,
                null, // claims
                options.NotBefore,
                options.Expiration,
                options.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}