using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    /// <summary>
    /// AutoMapper Profile for Schema (ServiceHandler) > Schema (AS4)
    /// </summary>
    public class SchemaMap : Profile
    {
        public SchemaMap()
        {
            CreateMap<Model.Common.Schema, Model.Core.Schema>()
                .ConstructUsing(submit => 
                    new Model.Core.Schema(
                        submit.Location, 
                        (submit.Version != null).ThenMaybe(submit.Version), 
                        (submit.Namespace != null).ThenMaybe(submit.Namespace)))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}