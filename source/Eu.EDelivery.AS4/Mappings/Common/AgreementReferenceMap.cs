using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class AgreementReferenceMap : Profile
    {
        public AgreementReferenceMap()
        {
            CreateMap<Model.Common.Agreement, Model.Core.AgreementReference>()
                .ConstructUsing(src => 
                    new Model.Core.AgreementReference(
                        src.Value, 
                        (src.RefType != null).ThenMaybe(src.RefType), 
                        (src.PModeId != null).ThenMaybe(src.PModeId)))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.AgreementReference, Model.Common.Agreement>()
                .ConstructUsing(model =>
                    new Model.Common.Agreement
                    {
                        Value = model.Value,
                        RefType = model.Type.GetOrElse(() => null),
                        PModeId = model.PModeId.GetOrElse(() => null)
                    })
                .ForAllOtherMembers(x => x.Ignore());

        }
    }
}