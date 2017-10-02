using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    public class TokenService : ITokenService
    {
        private readonly IOptionsSnapshot<JwtOptions> jwtOptions;
        private readonly UserManager<ApplicationUser> userManager;

        public TokenService(IOptionsSnapshot<JwtOptions> jwtOptions, UserManager<ApplicationUser> userManager)
        {
            this.jwtOptions = jwtOptions;
            this.userManager = userManager;
        }

        public async Task<string> GenerateToken(ApplicationUser user)
        {
            var options = jwtOptions.Value;
            var claims = await userManager.GetClaimsAsync(user);

            var jwt = new JwtSecurityToken(
                options.Issuer,
                options.Audience,
                claims,
                options.NotBefore,
                options.Expiration,
                options.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);          
        }
    }
}