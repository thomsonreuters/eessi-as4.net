using System;
using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class PartyIdMap : Profile
    {
        public PartyIdMap()
        {
            CreateMap<Model.Common.PartyId, Model.Core.PartyId>(MemberList.None)
                .ConstructUsing(src => 
                    string.IsNullOrEmpty(src.Type)
                        ? new Model.Core.PartyId(src.Id) 
                        : new Model.Core.PartyId(src.Id, src.Type));

            CreateMap<Model.Core.PartyId, Model.Common.PartyId>(MemberList.None)
                .ConstructUsing(src =>
                {
                    return new Model.Common.PartyId
                    {
                        Id = src.Id,
                        Type = src.Type.GetOrElse(() => String.Empty)
                    };
                });
        }
    }
}