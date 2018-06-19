using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class PayloadMap : Profile
    {
        public PayloadMap()
        {
            CreateMap<Model.Common.Payload, Model.Core.PartInfo>(MemberList.None)
                .ConstructUsing(src => new Model.Core.PartInfo(src.Id));

            CreateMap<Model.Core.PartInfo, Model.Common.Payload>()
                .ForMember(dest => dest.Id, src => src.MapFrom(t => t.Href))
                .AfterMap((corePartInfo, commonPayload) =>
                {
                    commonPayload.Id = corePartInfo.Href;

                    if (corePartInfo.Properties.ContainsKey("MimeType"))
                        commonPayload.MimeType = corePartInfo.Properties["MimeType"];

                    commonPayload.Location = string.Empty;
                    commonPayload.PayloadProperties = corePartInfo.Properties
                        .Select(CreatePayloadPropertyFromPair).ToArray();
                })
                .ForAllOtherMembers(x => x.Ignore());
        }

        private Model.Common.PayloadProperty CreatePayloadPropertyFromPair(KeyValuePair<string, string> pair)
        {
            return new Model.Common.PayloadProperty()
            {
                Name = pair.Key,
                Value = pair.Value
            };
        }
    }
}