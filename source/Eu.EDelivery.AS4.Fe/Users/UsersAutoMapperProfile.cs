using System.Linq;
using System.Security.Claims;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.Authentication;

namespace Eu.EDelivery.AS4.Fe.Users
{
    /// <summary>
    /// Setup Users automapper profile
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class UsersAutoMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsersAutoMapperProfile" /> class.
        /// </summary>
        public UsersAutoMapperProfile()
        {
            CreateMap<ApplicationUser, User>()
                .ForMember(x => x.Name, x => x.MapFrom(y => y.UserName))
                .ForMember(x => x.Roles, x => x.MapFrom(y => y.Claims.Select(z => z.ClaimValue)));
        }
    }
}