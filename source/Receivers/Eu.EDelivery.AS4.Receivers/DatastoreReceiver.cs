using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers.Specifications;
using NLog;

using Function = System.Func<
    Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
    System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.InternalMessage>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Receiver to poll the database to get the messages which validates a given Expression
    /// </summary>
    public class DatastoreReceiver : PollingTemplate<Entity, ReceivedMessage>, IReceiver
    {
        private readonly DatastoreSpecification _specification;

        private Func<DatastoreContext> _storeExpression;
        private Func<DatastoreContext, IEnumerable<Entity>> _findExpression;
        private Operation _operation;
        private IDictionary<string, string> _properties;

        protected override TimeSpan PollingInterval
        {
            get
            {
                TimeSpan defaultInterval = TimeSpan.FromSeconds(3);

                if (_properties == null)
                {
                    return defaultInterval;
                }

                return _properties.ContainsKey("PollingInterval") ? GetPollingIntervalFromProperties() : defaultInterval;
            }
        }

        private TimeSpan GetPollingIntervalFromProperties()
        {
            string pollingInterval = _properties.ReadMandatoryProperty("PollingInterval");
            double miliseconds = Convert.ToDouble(pollingInterval);

            return TimeSpan.FromMilliseconds(miliseconds);
        }

        protected override ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver"/> class
        /// </summary>
        public DatastoreReceiver()
        {
            _specification = new DatastoreSpecification();
            Logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver"/> class
        /// Create a Data Store Out Message Receiver with a given Data Store Context Delegate
        /// </summary>
        /// <param name="storeExpression">
        /// </param>
        /// <param name="findExpression">
        /// </param>
        /// <param name="updatedOperation">
        /// </param>
        public DatastoreReceiver(
            Func<DatastoreContext> storeExpression,
            Func<DatastoreContext, IEnumerable<Entity>> findExpression,
            Operation updatedOperation = Operation.NotApplicable)
        {
            _storeExpression = storeExpression;
            _findExpression = findExpression;
            _operation = updatedOperation;

            _specification = new DatastoreSpecification();
            Logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Configure the receiver with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        public void Configure(IDictionary<string, string> properties)
        {
            _properties = properties;
            _specification.Configure(properties);
            _findExpression = _specification.GetExpression().Compile();
            _storeExpression = () => new DatastoreContext(Config.Instance);

            if (properties.ContainsKey("Update"))
            {
                _operation = (Operation)Enum.Parse(typeof(Operation), properties["Update"]);
            }
            else
            {
                _operation = Operation.NotApplicable;
            }
        }

        /// <summary>
        /// Start Receiving on the Data Store
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(
            Function messageCallback,
            CancellationToken cancellationToken)
        {
            LogReceiverSpecs();
            StartPolling(messageCallback, cancellationToken);
        }

        public void StopReceiving()
        {
            if (_properties == null)
            {
                return;
            }

            string table = _properties["Table"];
            string field = _properties["Field"];
            string value = _properties["Value"];

            Logger.Debug($"Stop Receiving on Datastore FROM {table} WHERE {field} == {value}");
        }

        private void LogReceiverSpecs()
        {
            if (_properties == null)
            {
                return;
            }

            string table = _properties["Table"];
            string field = _properties["Field"];
            string value = _properties["Value"];

            Logger.Debug($"Start Receiving on Datastore FROM {table} WHERE {field} == {value}");
        }

        /// <summary>
        /// Get the Out Messages from the Store with <see cref="Operation.ToBeSent" /> as Operation
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override IEnumerable<Entity> GetMessagesToPoll(CancellationToken cancellationToken)
        {
            LogManager.GetCurrentClassLogger().Trace($"Executing GetMessagesToPoll on {_properties?["Table"]}");

            try
            {                
                IEnumerable<Entity> entities;

                using (DatastoreContext context = _storeExpression())
                {
                    var tx = context.Database.BeginTransaction();
                    
                    try
                    {
                        entities = _findExpression(context).ToList();

                        if (entities.Any())
                        {
                            // Make sure that all message-entities are locked before continue to process them.
                            if (_operation != Operation.NotApplicable)
                            {
                                foreach (var messageEntity in entities.OfType<MessageEntity>())
                                {
                                    messageEntity.Operation = _operation;
                                }
                            }
                            context.SaveChanges();
                        }

                        tx.Commit();
                    }
                    catch(Exception ex)
                    {
                        Logger.Error(ex.Message);
                        tx.Rollback();
                        throw;
                    }
                }

                return entities;
            }
            catch (Exception exception)
            {
                var logger = LogManager.GetCurrentClassLogger();

                logger.Error($"An error occured while polling the datastore: {exception.Message}");
                if (_properties != null)
                {
                    logger.Error($"Polling on table {_properties["Table"]} with interval {PollingInterval.TotalSeconds} seconds.");
                }
                logger.Error(exception.StackTrace);
                return new Entity[] { };
            }

        }

        /// <summary>
        /// Describe what to do when a Out Message is received
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="messageCallback"></param>
        /// <param name="token"></param>
        protected override void MessageReceived(Entity entity, Function messageCallback, CancellationToken token)
        {
            var messageEntity = entity as MessageEntity;
            if (messageEntity != null)
            {
                ReceiveMessageEntity(messageEntity, messageCallback, token);
            }
            else
            {
                ReceiveEntity(entity, messageCallback, token);
            }
        }

        private void ReceiveMessageEntity(MessageEntity messageEntity, Function messageCallback, CancellationToken token)
        {
            Logger.Info($"Received Message from Datastore with Ebms Message Id: {messageEntity.EbmsMessageId}");

            using (var memoryStream = new MemoryStream(messageEntity.MessageBody))
            {
                ReceivedMessage receivedMessage = CreateReceivedMessage(messageEntity, memoryStream);
                messageCallback(receivedMessage, token);
            }
        }

        private static ReceivedMessage CreateReceivedMessage(MessageEntity messageEntity, Stream stream)
        {
            return new ReceivedMessageEntityMessage(messageEntity: messageEntity)
            {
                RequestStream = stream,
                ContentType = messageEntity.ContentType
            };
        }

        private static void ReceiveEntity(Entity entity, Function messageCallback, CancellationToken token)
        {
            var message = new ReceivedEntityMessage(entity);
            messageCallback(message, token);
        }

        protected override void HandleMessageException(Entity message, Exception exception)
        {
            Logger.Error(exception.Message);
            var aggregate = exception as AggregateException;
            if (aggregate != null)
            {
                foreach (var ex in aggregate.InnerExceptions)
                {
                    Logger.Error(ex.Message);
                }
            }
        }

        protected override void ReleasePendingItems()
        {
            // TODO: we should release the records that have been held locked by this
            // DataStoreReceiver so that they won't be locked forever.
        }
    }

}