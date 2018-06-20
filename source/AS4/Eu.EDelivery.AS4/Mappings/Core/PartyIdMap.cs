using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class PartyIdMap : Profile
    {
        public PartyIdMap()
        {
            CreateMap<Model.Core.PartyId, Xml.PartyId>(MemberList.None)
                .ConstructUsing(model => 
                    string.Empty == model.Type
                        ? new Xml.PartyId { Value = model.Id }
                        : new Xml.PartyId { Value = model.Id, type = model.Type });

            CreateMap<Xml.PartyId, Model.Core.PartyId>(MemberList.None)
                .ConstructUsing(xml =>
                    string.IsNullOrEmpty(xml.type)
                        ? new Model.Core.PartyId(xml.Value)
                        : new Model.Core.PartyId(xml.Value, xml.type));

            CreateMap<Xml.PartyId[], Model.Core.PartyId>(MemberList.None)
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}