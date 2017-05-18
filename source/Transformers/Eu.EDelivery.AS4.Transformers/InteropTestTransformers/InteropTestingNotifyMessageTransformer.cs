using System.Diagnostics.CodeAnalysis;

namespace Eu.EDelivery.AS4.Transformers.InteropTestTransformers
{
    [ExcludeFromCodeCoverage]
    public class InteropTestingNotifyMessageTransformer : MinderNotifyMessageTransformer
    {
        protected override string MinderUriPrefix => "http://www.esens.eu/as4/interoptest";
    }
}
