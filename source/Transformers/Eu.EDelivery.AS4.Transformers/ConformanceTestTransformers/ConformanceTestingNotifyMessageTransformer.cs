using System.Diagnostics.CodeAnalysis;

namespace Eu.EDelivery.AS4.Transformers.ConformanceTestTransformers
{
    [ExcludeFromCodeCoverage]
    public class ConformanceTestingNotifyMessageTransformer : MinderNotifyMessageTransformer
    {
        protected override string MinderUriPrefix => "http://www.esens.eu/as4/conformancetest";
    }
}
