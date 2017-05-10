using System.Diagnostics.CodeAnalysis;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;

namespace Eu.EDelivery.AS4.Transformers.InteropTestTransformers
{
    [ExcludeFromCodeCoverage]
    public class InteropTestingExceptionNotifyMessageTransformer : ExceptionToNotifyMessageTransformer
    {
        protected override NotifyMessageEnvelope CreateNotifyMessageEnvelope(AS4Message as4Message)
        {
            var notifyTransformer = new InteropTestingNotifyMessageTransformer();

            return notifyTransformer.CreateNotifyMessageEnvelope(as4Message).Result;
        }
    }
}
