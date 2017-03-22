using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4ReceiptStep"/> class
        /// </summary>
        public StoreAS4ReceiptStep()
        {
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
            if (internalMessage.AS4Message.IsEmpty)
            {
                return await StepResult.SuccessAsync(internalMessage);
            }

            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                await StoreReceiptInDatastore(internalMessage.AS4Message, new OutMessageService(repository));
                _logger.Info($"{internalMessage.Prefix} Store AS4 Receipt into the Datastore");
            }

            return await StepResult.SuccessAsync(internalMessage);
        }

        private static async Task StoreReceiptInDatastore(AS4Message as4Message, OutMessageService service)
        {
            // TODO: what if there are multiple Receipts in our message ? (Bundling)            
            await service.InsertReceiptAsync(as4Message);
        }
    }
}
