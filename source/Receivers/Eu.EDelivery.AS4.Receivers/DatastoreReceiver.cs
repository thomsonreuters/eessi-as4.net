using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers.Specifications;
using Eu.EDelivery.AS4.Receivers.Specifications.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using Function =
    System.Func<Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.MessagingContext>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Receiver to poll the database to get the messages which validates a given Expression
    /// </summary>
    public class DatastoreReceiver : PollingTemplate<Entity, ReceivedMessage>, IReceiver
    {
        private readonly IDictionary<string, string> _properties;
        private readonly IDatastoreSpecification _specification;
        private readonly Func<DatastoreContext> _storeExpression;
        private readonly IDictionary<string, string> _updates;

        private Func<DatastoreContext, IEnumerable<Entity>> _findExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver" /> class
        /// </summary>
        public DatastoreReceiver() : this(() => new DatastoreContext(Config.Instance)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver" /> class.
        /// </summary>
        /// <param name="storeExpression"></param>
        public DatastoreReceiver(Func<DatastoreContext> storeExpression)
        {
            _storeExpression = storeExpression;
            _specification = new ExpressionDatastoreSpecification();
            _updates = new Dictionary<string, string>();
            _properties = new Dictionary<string, string>();
        }

        protected override ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Start Receiving on the Data Store
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            LogReceiverSpecs(true);
            StartPolling(messageCallback, cancellationToken);
        }

        public void StopReceiving()
        {
            LogReceiverSpecs(false);
        }

        private void LogReceiverSpecs(bool startReceiving)
        {
            string action = startReceiving ? "Start" : "Stop";
            Logger.Debug($"{action} Receiving on Datastore {_specification.FriendlyExpression}");
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
                    LogExceptionAndInner(exception);
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

            if (!_updates.Any())
            {
                Logger.Warn(
                    $"No UpdateValue configured for {_properties[SettingKeys.Filter]}. "
                    + $"The entities retrieved from {_properties[SettingKeys.Table]} are not being locked.");

                return entities;
            }

            LockEntitiesBeforeContinueToProcessThem(entities);

            context.SaveChanges();
            return entities;
        }

        private void LockEntitiesBeforeContinueToProcessThem(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                foreach (KeyValuePair<string, string> update in _updates)
                {
                    PropertyInfo property = entity.GetType().GetProperty(update.Key);

                    object propertyValue = property.GetValue(entity);
                    object updateValue = Conversion.Convert(propertyValue, update.Value);

                    property.SetValue(entity, updateValue);
                }
            }
        }

        private async void ReceiveMessageEntity(
            MessageEntity messageEntity,
            Function messageCallback,
            CancellationToken token)
        {
            Logger.Info($"Received Message from Datastore with Ebms Message Id: {messageEntity.EbmsMessageId}");

            using (Stream stream = await messageEntity.RetrieveMessagesBody(Registry.Instance.MessageBodyStore))
            {
                if (stream == null)
                {
                    Logger.Error($"MessageBody cannot be retrieved for Ebms Message Id: {messageEntity.EbmsMessageId}");
                }
                else
                {
                    ReceivedMessage receivedMessage = CreateReceivedMessage(messageEntity, stream);
                    try
                    {
                        await messageCallback(receivedMessage, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        receivedMessage.UnderlyingStream.Dispose();
                    }
                }
            }
        }

        private static ReceivedMessage CreateReceivedMessage(MessageEntity messageEntity, Stream stream)
        {
            return new ReceivedMessageEntityMessage(messageEntity, stream, messageEntity.ContentType);
        }

        private static async void ReceiveEntity(Entity entity, Function messageCallback, CancellationToken token)
        {
            var message = new ReceivedEntityMessage(entity);
            MessagingContext result = await messageCallback(message, token).ConfigureAwait(false);
            result?.Dispose();
        }

        private void LogExceptionAndInner(Exception exception)
        {
            Logger.Error(exception.Message);
            Logger.Trace(exception.StackTrace);

            if (exception.InnerException != null)
            {
                Logger.Error(exception.InnerException.Message);
                Logger.Trace(exception.InnerException.StackTrace);
            }
        }

        #region Configuration

        [Info("Table", required: true)]
        private string Table => _properties?.ReadOptionalProperty(SettingKeys.Table);

        [Info("Filter", required: true)]
        private string Filter => _properties?.ReadOptionalProperty(SettingKeys.Filter);

        [Info("How many rows to take", defaultValue: SettingKeys.TakeRowsDefault, type: "int32")]
        private int TakeRows => Convert.ToInt32(_properties?.ReadOptionalProperty(SettingKeys.TakeRows));

        [Info("Update", attributes: new[] { "Field" })]
        private string Update => _properties?.ReadOptionalProperty(SettingKeys.Update);

        private static class SettingKeys
        {
            public const string PollingInterval = "PollingInterval";
            public const string Table = "Table";
            public const string Filter = "Filter";
            public const string TakeRows = "BatchSize";
            public const string TakeRowsDefault = "20";
            public const string Update = "Update";
            public const int PollingIntervalDefault = 3;
        }

        [Info("Polling interval", null, "int32", SettingKeys.PollingIntervalDefault)]
        protected override TimeSpan PollingInterval
        {
            get
            {
                return GetPollingIntervalFromProperties();
            }
        }

        private TimeSpan GetPollingIntervalFromProperties()
        {
            TimeSpan defaultInterval = TimeSpan.FromSeconds(3);

            if (_properties.ContainsKey(SettingKeys.PollingInterval) == false)
            {
                return defaultInterval;
            }

            string pollingInterval = _properties[SettingKeys.PollingInterval];
            return pollingInterval.AsTimeSpan(defaultInterval);            
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(IEnumerable<Setting> settings)
        {
            Configure(
                settings.GroupBy(s => s.Key, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(s => s.Key, s => s.First().Value, StringComparer.OrdinalIgnoreCase));

            RetrieveUpdates(settings)?.ToList().ForEach(_updates.Add);
        }

        /// <summary>
        /// Configure the receiver with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        private void Configure(IDictionary<string, string> properties)
        {
            properties.ToList().ForEach(_properties.Add);
            _specification.Configure(CreateDatastoreArgsFrom(_properties));
            _findExpression = _specification.GetExpression().Compile();
        }

        private static DatastoreSpecificationArgs CreateDatastoreArgsFrom(IDictionary<string, string> properties)
        {
            string tableName = properties.ReadMandatoryProperty(SettingKeys.Table);
            string filterColumn = properties.ReadMandatoryProperty(SettingKeys.Filter);
            int take = Convert.ToInt32(properties.ReadOptionalProperty(SettingKeys.TakeRows, SettingKeys.TakeRowsDefault));

            return new DatastoreSpecificationArgs(tableName, filterColumn, take);
        }

        private static IDictionary<string, string> RetrieveUpdates(IEnumerable<Setting> settings)
        {
            return settings.Where(s => s.Key.Equals("Update")).ToDictionary(s => s["field"].Value, s => s.Value);
        }

        #endregion

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

                return Enumerable.Empty<Entity>();
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
                LogExceptionAndInner(ex);
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