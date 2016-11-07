using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how a AS4 Receipt gets stores in the Data store
    /// </summary>
    public class StoreAS4ReiptStep : IStep
    {
        private readonly IOutMessageService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4ReiptStep"/> class
        /// </summary>
        public StoreAS4ReiptStep()
        {
            this._service = new OutMessageService(Registry.Instance.DatastoreRepository);
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4ReiptStep"/> class
        /// Create an <see cref="IStep"/> implementation
        /// to store AS4 Receipts into the Data store
        /// </summary>
        /// <param name="service"> </param>
        public StoreAS4ReiptStep(IOutMessageService service)
        {
            this._service = service;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start storing the AS4 Receipt
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            await StoreReceiptInDatastore(internalMessage.AS4Message);
            this._logger.Info($"{internalMessage.Prefix} Store AS4 Receipt into the Datastore");

            return StepResult.Success(internalMessage);
        }

        private async Task StoreReceiptInDatastore(AS4Message as4Message)
        {
            string messageId = as4Message.PrimarySignalMessage.MessageId;
            await this._service.InsertReceiptAsync(messageId, as4Message);
        }
    }
}
