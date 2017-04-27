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
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using Function =
    System.Func<Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
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
        private string _updateValue;
        private IDictionary<string, string> _properties;

        protected override ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver"/> class
        /// </summary>
        public DatastoreReceiver()
        {
            _specification = new DatastoreSpecification();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver"/> class.
        /// Create a Data Store Out Message Receiver with a given Data Store Context Delegate
        /// </summary>
        /// <param name="storeExpression">
        /// </param>
        /// <param name="findExpression">
        /// </param>
        /// <param name="updateValue">
        /// </param>
        public DatastoreReceiver(
            Func<DatastoreContext> storeExpression,
            Func<DatastoreContext, IEnumerable<Entity>> findExpression,
            string updateValue)
        {
            _storeExpression = storeExpression;
            _findExpression = findExpression;

            _specification = new DatastoreSpecification();
            _updateValue = updateValue;
        }

        #region Configuration

        private static class SettingKeys
        {
            public const string PollingInterval = "PollingInterval";
            public const string Table = "Table";
            public const string Field = "Field";
            public const string FilterValue = "Value";
            public const string TakeRows = "BatchSize";
            public const string UpdateValue = "Update";
        }

        protected override TimeSpan PollingInterval
        {
            get
            {
                TimeSpan defaultInterval = TimeSpan.FromSeconds(3);

                if (_properties == null)
                {
                    return defaultInterval;
                }

                return _properties.ContainsKey(SettingKeys.PollingInterval) ? GetPollingIntervalFromProperties() : defaultInterval;
            }
        }

        private TimeSpan GetPollingIntervalFromProperties()
        {
            string pollingInterval = _properties.ReadMandatoryProperty(SettingKeys.PollingInterval);
            double miliseconds = Convert.ToDouble(pollingInterval);

            return TimeSpan.FromMilliseconds(miliseconds);
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(IEnumerable<Setting> settings)
        {
            Configure(settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Configure the receiver with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        private void Configure(IDictionary<string, string> properties)
        {
            _properties = properties;

            var args = new DatastoreSpecificationArgs(_properties.ReadMandatoryProperty(SettingKeys.Table),
                                                      _properties.ReadMandatoryProperty(SettingKeys.Field),
                                                      _properties.ReadMandatoryProperty(SettingKeys.FilterValue),
                                                      Convert.ToInt32(_properties.ReadOptionalProperty(SettingKeys.TakeRows, "20")));

            _specification.Configure(args);
            _findExpression = _specification.GetExpression().Compile();
            _storeExpression = () => new DatastoreContext(Config.Instance);

            properties.TryGetValue(SettingKeys.UpdateValue, out _updateValue);
        }

        #endregion

        /// <summary>
        /// Start Receiving on the Data Store
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            LogReceiverSpecs(startReceiving: true);
            StartPolling(messageCallback, cancellationToken);
        }

        public void StopReceiving()
        {
            LogReceiverSpecs(startReceiving: false);
        }

        private void LogReceiverSpecs(bool startReceiving)
        {
            if (_properties == null)
            {
                return;
            }

            string table = _properties[SettingKeys.Table];
            string field = _properties[SettingKeys.Field];
            string value = _properties[SettingKeys.FilterValue];

            string action = startReceiving ? "Start" : "Stop";

            Logger.Debug($"{action} Receiving on Datastore FROM {table} WHERE {field} == {value}");
        }

        /// <summary>
        /// Get the Out Messages from the Store with <see cref="Operation.ToBeSent" /> as Operation
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override IEnumerable<Entity> GetMessagesToPoll(CancellationToken cancellationToken)
        {
            try
            {
                return GetMessagesEntitiesForConfiguredExpression();
            }
            catch (Exception exception)
            {
                Logger logger = LogManager.GetCurrentClassLogger();

                logger.Error($"An error occured while polling the datastore: {exception.Message}");
                logger.Error(
                    $"Polling on table {_properties.ReadMandatoryProperty(SettingKeys.Table)} with interval {PollingInterval.TotalSeconds} seconds.");
                logger.Error(exception.StackTrace);

                return new Entity[] { };
            }
        }

        private IEnumerable<Entity> GetMessagesEntitiesForConfiguredExpression()
        {
            // Use a TransactionScope to get the highest TransactionIsolation level.
            IEnumerable<Entity> entities = Enumerable.Empty<Entity>();

            using (DatastoreContext context = _storeExpression())
            {
                IDbContextTransaction transaction = context.Database.BeginTransaction();

                try
                {
                    entities = FindAnyMessageEntitiesWithConfiguredExpression(context);
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.Message);
                    transaction.Rollback();
                }
            }

            return entities;
        }

        private IEnumerable<Entity> FindAnyMessageEntitiesWithConfiguredExpression(DatastoreContext context)
        {
            IEnumerable<Entity> entities = _findExpression(context).ToList();
            if (!entities.Any())
            {
                return entities;
            }

            if (_updateValue == null)
            {
                Logger.Warn($"No UpdateValue configured for {_properties[SettingKeys.Field]}. The entities retrieved from {_properties[SettingKeys.Table]} are not being locked.");
                return entities;
            }

            // Make sure that all message-entities are locked before continue to process them.
            foreach (Entity entity in entities)
            {
                entity.Lock(_updateValue);
            }

            context.SaveChanges();
            return entities;
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

        private async void ReceiveMessageEntity(MessageEntity messageEntity, Function messageCallback, CancellationToken token)
        {
            Logger.Info($"Received Message from Datastore with Ebms Message Id: {messageEntity.EbmsMessageId}");

            using (Stream stream = messageEntity.RetrieveMessageBody(Registry.Instance.MessageBodyRetrieverProvider))
            {
                if (stream == null)
                {
                    Logger.Error($"MessageBody cannot be retrieved for Ebms Message Id: {messageEntity.EbmsMessageId}");
                }
                else
                {
                    ReceivedMessage receivedMessage = CreateReceivedMessage(messageEntity, stream);
                    await messageCallback(receivedMessage, token);
                }
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

        private static async void ReceiveEntity(Entity entity, Function messageCallback, CancellationToken token)
        {
            var message = new ReceivedEntityMessage(entity);
            InternalMessage result = await messageCallback(message, token);
            result?.Dispose();
        }

        protected override void HandleMessageException(Entity message, Exception exception)
        {
            Logger.Error(exception.Message);
            var aggregate = exception as AggregateException;
            if (aggregate == null)
            {
                return;
            }

            foreach (Exception ex in aggregate.InnerExceptions)
            {
                Logger.Error(ex.Message);
            }
        }

        protected override void ReleasePendingItems()
        {
            // TODO: we should release the records that have been held locked by this
            // DataStoreReceiver so that they won't be locked forever.
            // -> Reset the records that have been locked by this process and who'sestatus is still the same.
        }
    }
}