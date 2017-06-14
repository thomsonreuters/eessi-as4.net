using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Common;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// TODO: temporary solution for handling different 'unhappy' paths
    /// </summary>
    /// <seealso cref="OutExceptionStepDecorator" />
    public class PullOutExceptionStepDecorator : OutExceptionStepDecorator
    {
        private readonly IStep _innerStep;
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullOutExceptionStepDecorator" /> class.
        /// </summary>
        /// <param name="innerStep">The inner step.</param>
        /// <param name="createContext">The create context.</param>
        public PullOutExceptionStepDecorator(IStep innerStep, Func<DatastoreContext> createContext) : base(innerStep)
        {
            _innerStep = innerStep;
            _createContext = createContext;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<StepResult> ExecuteAsync(
            MessagingContext messagingContext,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _innerStep.ExecuteAsync(messagingContext, cancellationToken);
            }
            catch (PullRequestValidationException exception)
            {
                InsertOutException(messagingContext, exception);

                AS4Message as4Message = BuildAS4Error(messagingContext, exception);

                // TODO: ugly hack
                var pmode = new ReceivingProcessingMode {ErrorHandling = {ResponseHttpCode = 403}};
                return StepResult.Success(new MessagingContext(as4Message, MessagingContextMode.Send) {ReceivingPMode = pmode});
            }
            catch (AS4Exception exception)
            {
                return await HandleAS4Exception(messagingContext, exception, cancellationToken);
            }
        }

        /// <summary>
        /// Inserts the out exception.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        private void InsertOutException(MessagingContext context, AS4Exception exception)
        {
            OutException outException = OutExceptionBuilder.ForAS4Exception(exception).Build();
            outException.MessageBody = AS4XmlSerializer.ToBytes(context.AS4Message);

            using (DatastoreContext datastoreContext = _createContext())
            {
                datastoreContext.OutExceptions.Add(outException);
                datastoreContext.SaveChanges();
            }
        }

        private static AS4Message BuildAS4Error(MessagingContext messagingContext, AS4Exception exception)
        {
            Error error = new ErrorBuilder()
                .WithRefToEbmsMessageId(messagingContext.AS4Message.GetPrimaryMessageId())
                .WithAS4Exception(exception)
                .Build();

            return AS4Message.Create(error);
        }
    }
}