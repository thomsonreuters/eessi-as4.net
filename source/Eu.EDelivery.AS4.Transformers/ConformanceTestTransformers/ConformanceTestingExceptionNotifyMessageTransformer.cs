using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;

namespace Eu.EDelivery.AS4.Transformers.ConformanceTestTransformers
{
    [NotConfigurable]
    [ExcludeFromCodeCoverage]
    public class ConformanceTestingExceptionNotifyMessageTransformer : NotifyMessageTransformer
    {
        protected override async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelopeAsync(
            AS4Message as4Message, 
            string receivedEntityMessageId,
            Type receivedEntityType)
        {
            var notifyTransformer = new ConformanceTestingNotifyMessageTransformer();

            return await notifyTransformer.CreateNotifyMessageEnvelope(as4Message, receivedEntityMessageId, receivedEntityType);
        }
    }
}
