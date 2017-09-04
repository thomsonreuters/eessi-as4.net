using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;

namespace Eu.EDelivery.AS4.Transformers.ConformanceTestTransformers
{
    [ExcludeFromCodeCoverage]
    public class ConformanceTestingExceptionNotifyMessageTransformer : ExceptionToNotifyMessageTransformer
    {
        protected override async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message, Type receivedEntityType)
        {
            var notifyTransformer = new ConformanceTestingNotifyMessageTransformer();

            return await notifyTransformer.CreateNotifyMessageEnvelope(as4Message, receivedEntityType);
        }
    }
}
