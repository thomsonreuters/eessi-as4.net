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
using Eu.EDelivery.AS4.Steps.Common;

namespace Eu.EDelivery.AS4.Steps.Send
{
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
            catch (PullException exception)
            {
                InsertOutException(messagingContext, exception);

                AS4Message as4Message = BuildAS4Error(messagingContext, exception);
                return StepResult.Success(new MessagingContext(as4Message));
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
            outException.MessageBody = context.AS4Message.AsBytes();

            _createContext().Using(
                c =>
                {
                    c.OutExceptions.Add(outException);
                    c.SaveChanges();
                });
        }

        private static AS4Message BuildAS4Error(MessagingContext messagingContext, AS4Exception exception)
        {
            Error error = new ErrorBuilder()
                .WithRefToEbmsMessageId(messagingContext.AS4Message.GetPrimaryMessageId())
                .WithAS4Exception(exception)
                .Build();

            return new AS4MessageBuilder().WithSignalMessage(error).Build();
        }
    }
}