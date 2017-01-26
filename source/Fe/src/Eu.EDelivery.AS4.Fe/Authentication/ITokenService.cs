using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    public interface ITokenService
    {
        Task<string> GenerateToken(ApplicationUser user);
    }
}