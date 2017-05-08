using System.Diagnostics.CodeAnalysis;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;

namespace Eu.EDelivery.AS4.Transformers.ConformanceTestTransformers
{
    [ExcludeFromCodeCoverage]
    public class ConformanceTestingExceptionNotifyMessageTransformer : ExceptionToNotifyMessageTransformer
    {
        protected override NotifyMessageEnvelope CreateNotifyMessageEnvelope(AS4Message as4Message)
        {
            var notifyTransformer = new ConformanceTestingNotifyMessageTransformer();

            return notifyTransformer.CreateNotifyMessageEnvelope(as4Message).Result;
        }
    }
}
