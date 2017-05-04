using System;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Services
{
    public interface IOutExceptionService
    {
        void InsertAS4Exception(AS4Exception as4Exception, AS4Message as4Message);
    }

    public class OutExceptionService : IOutExceptionService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DatastoreContext _context;
        /// <summary>
        /// Initializes a new instance of the <see cref="OutExceptionService"/> class.
        /// </summary>
        public OutExceptionService(DatastoreContext context)
        {
            _context = context;
        }

        public void InsertAS4Exception(AS4Exception as4Exception, AS4Message as4Message)
        {
            var repository = new DatastoreRepository(_context);

            foreach (string id in as4Message.MessageIds)
            {
                try
                {
                    OutException outException = CreateOutException(as4Exception, id, as4Message);
                    repository.InsertOutException(outException);
                }
                catch (Exception exception)
                {
                    Logger.Error("Cannot Update Datastore with InException");
                    Logger.Debug($"Cannot update Datastore: {exception.Message}");
                }
            }            
        }

        private static OutException CreateOutException(AS4Exception as4Exception, string messageId, AS4Message as4Message)
        {
            OutExceptionBuilder builder = OutExceptionBuilder.ForAS4Exception(as4Exception).WithEbmsMessageId(messageId);

            if (NeedsOutExceptionBeNotified(as4Message.SendingPMode))
            {
                builder.WithOperation(Operation.ToBeNotified);
            }

            return builder.Build();
        }

        private static bool NeedsOutExceptionBeNotified(SendingProcessingMode sendPMode)
        {
            return sendPMode?.ExceptionHandling?.NotifyMessageProducer == true;
        }
    }
}