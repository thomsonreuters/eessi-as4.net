using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class AgreementRefMap : Profile
    {
        public AgreementRefMap()
        {
            CreateMap<Model.Core.AgreementReference, Xml.AgreementRef>()
                .ConstructUsing(model => 
                    new Xml.AgreementRef
                    {
                        Value = model.Value,
                        type = model.Type.GetOrElse(() => null),
                        pmode = model.PModeId.GetOrElse(() => null)
                    })
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.AgreementRef, Model.Core.AgreementReference>()
                .ConstructUsing(xml => 
                    new Model.Core.AgreementReference(
                        xml.Value, 
                        (xml.type != null).ThenMaybe(xml.type), 
                        (xml.pmode != null).ThenMaybe(xml.pmode)))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}