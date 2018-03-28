using AutoMapper;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.SmpConfiguration
{
    /// <summary>
    ///     AutoMapper setup for <seealso cref="AS4.Entities.SmpConfiguration" />
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class SmpConfigurationAutoMapperProfile : Profile
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SmpConfigurationAutoMapperProfile" /> class.
        /// </summary>
        public SmpConfigurationAutoMapperProfile()
        {
            CreateMap<SmpConfiguration, Entities.SmpConfiguration>()
                .ForMember(x => x.EncryptPublicKeyCertificate, x => x.Ignore())
                .ForMember(x => x.Id, x => x.Ignore());
        }
    }
}