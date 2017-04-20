using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how a AS4 Receipt gets stores in the Data store
    /// </summary>
    public class StoreAS4ReceiptStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IAS4MessageBodyPersister _messageBodyPersister;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4ReceiptStep"/> class.
        /// </summary>
        public StoreAS4ReceiptStep() : this(Config.Instance.OutgoingAS4MessageBodyPersister)
        {                
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4ReceiptStep"/> class
        /// </summary>
        public StoreAS4ReceiptStep(IAS4MessageBodyPersister messageBodyPersister)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _messageBodyPersister = messageBodyPersister;
        }

        /// <summary>
        /// Start storing the AS4 Receipt
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (internalMessage.AS4Message.IsEmpty)
            {
                return await StepResult.SuccessAsync(internalMessage);
            }

            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                new OutMessageService(repository, _messageBodyPersister).InsertReceipt(internalMessage.AS4Message);
                
                await context.SaveChangesAsync(cancellationToken);

                _logger.Info($"{internalMessage.Prefix} Store AS4 Receipt into the Datastore");
            }

            return await StepResult.SuccessAsync(internalMessage);
        }        
    }
}
