using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Services
{
    public class OutExceptionService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DatastoreContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutExceptionService" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public OutExceptionService(DatastoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Inserts the error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="as4Message">The context.</param>
        public async Task InsertException(ErrorResult error, AS4Message as4Message, SendingProcessingMode sendingPMode)
        {
            var repository = new DatastoreRepository(_context);

            foreach (string id in as4Message.MessageIds)
            {
                try
                {
                    OutException outException = await CreateOutExceptionAsync(error, sendingPMode, id);

                    repository.InsertOutException(outException);
                }
                catch (Exception exception)
                {
                    Logger.Error("Cannot Update Datastore with InException");
                    Logger.Debug($"Cannot update Datastore: {exception.Message}");
                }
            }
        }

        private static bool NeedsOutExceptionBeNotified(SendingProcessingMode sendPMode)
        {
            return sendPMode?.ExceptionHandling?.NotifyMessageProducer == true;
        }

        private static async Task<OutException> CreateOutExceptionAsync(ErrorResult error, SendingProcessingMode sendingPMode, string id)
        {
            var outException = new OutException(id, error.Description);
            
            await outException.SetPModeInformationAsync(sendingPMode);
            outException.SetOperation(NeedsOutExceptionBeNotified(sendingPMode) ? Operation.ToBeNotified : Operation.NotApplicable);

            return outException;
        }
    }
}