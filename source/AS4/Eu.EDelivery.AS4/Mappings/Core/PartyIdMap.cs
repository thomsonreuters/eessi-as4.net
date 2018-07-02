using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class PartyIdMap : Profile
    {
        public PartyIdMap()
        {
            CreateMap<Model.Core.PartyId, Xml.PartyId>()
                .ConstructUsing(model => 
                    new Xml.PartyId
                    {
                        Value = model.Id,
                        type = model.Type.GetOrElse(() => null)
                    })
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.PartyId, Model.Core.PartyId>()
                .ConstructUsing(xml =>
                    xml.type == null
                        ? new Model.Core.PartyId(xml.Value)
                        : new Model.Core.PartyId(xml.Value, xml.type))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.PartyId[], Model.Core.PartyId>(MemberList.None)
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}