using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// <see cref="ITransformer"/> implementation that's responsible for transformation PMode models to Pull Messages instances.
    /// </summary>
    public class PModeToPullMessageTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="InternalMessage"/> instance.
        /// </summary>
        /// <param name="receivedMessage">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public Task<InternalMessage> TransformAsync(ReceivedMessage receivedMessage, CancellationToken cancellationToken)
        {
            if (receivedMessage.RequestStream == null)
            {
                return Task.FromResult(new InternalMessage(CreateAS4Exception("Invalid incoming request stream.")));
            }

            return TryCreatePullRequest(receivedMessage, cancellationToken);
        }

        private static Task<InternalMessage> TryCreatePullRequest(ReceivedMessage receivedMessage, CancellationToken cancellationToken)
        {
            var transformedMessage = new InternalMessage();

            try
            {
                receivedMessage.AssignProperties(transformedMessage.AS4Message);

                if (transformedMessage.AS4Message.PrimarySignalMessage == null)
                    throw CreateAS4Exception("Invalid incoming received message");
            }
            catch (AS4Exception exception)
            {
                transformedMessage.Exception = exception;
            }
            catch (Exception exception)
            {
                transformedMessage.Exception = CreateAS4Exception(exception.Message);
            }

            return Task.FromResult(transformedMessage);
        }

        private static AS4Exception CreateAS4Exception(string description)
        {
            Logger.Error(description);
            return AS4ExceptionBuilder.WithDescription(description).Build();
        }
    }
}