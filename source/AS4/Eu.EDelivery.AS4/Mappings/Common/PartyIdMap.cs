using System;
using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class PartyIdMap : Profile
    {
        public PartyIdMap()
        {
            CreateMap<Model.Common.PartyId, Model.Core.PartyId>()
                .ConstructUsing(src => 
                    src.Type == null
                        ? new Model.Core.PartyId(src.Id) 
                        : new Model.Core.PartyId(src.Id, src.Type))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.PartyId, Model.Common.PartyId>()
                .ConstructUsing(src =>
                {
                    return new Model.Common.PartyId
                    {
                        Id = src.Id,
                        Type = src.Type.GetOrElse(() => null)
                    };
                })
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}