using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers.Utilities;
using Microsoft.EntityFrameworkCore;
using NLog;
using Function =
    System.Func<Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.MessagingContext>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Receiver to poll the database to get the messages which validates a given Expression
    /// </summary>
    [Info("Datastore receiver")]
    public class DatastoreReceiver : PollingTemplate<Entity, ReceivedMessage>, IReceiver
    {
        private readonly Func<DatastoreContext> _storeExpression;
        private readonly Func<DatastoreContext, IEnumerable<Entity>> _retrieveEntities;

        private DatastoreReceiverSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver" /> class
        /// </summary>
        public DatastoreReceiver() : this(() => new DatastoreContext(Config.Instance)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver" /> class.
        /// </summary>
        /// <param name="storeExpression">The store expression.</param>
        public DatastoreReceiver(Func<DatastoreContext> storeExpression)
        {
            _storeExpression = storeExpression;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver"/> class.
        /// </summary>
        public DatastoreReceiver(
            Func<DatastoreContext> storeExpression,
            Func<DatastoreContext, IEnumerable<Entity>> retrieveEntities)
        {
            _storeExpression = storeExpression;
            _retrieveEntities = retrieveEntities;
        }

        protected override ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Start Receiving on the Data Store
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            if (_settings == null && _retrieveEntities == null)
            {
                throw new InvalidOperationException("The DatastoreReceiver is not configured");
            }

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
            Logger.Debug($"{action} Receiving on Datastore {_settings.DisplayString}");
        }

        private IEnumerable<Entity> GetMessagesEntitiesForConfiguredExpression()
        {
            using (DatastoreContext context = _storeExpression())
            {
                if (context.NativeCommands.ExclusiveLockIsolation.HasValue)
                {
                    context.Database.BeginTransaction(context.NativeCommands.ExclusiveLockIsolation.Value);
                }
                else
                {
                    context.Database.BeginTransaction();
                }

                try
                {
                    IEnumerable<Entity> entities = FindAnyMessageEntitiesWithConfiguredExpression(context);
                    context.Database.CommitTransaction();
                    return entities;
                }
                catch (Exception exception)
                {
                    context.Database.RollbackTransaction();
                    LogExceptionAndInner(exception);
                }

                return Enumerable.Empty<Entity>();
            }
        }

        // ReSharper disable once InconsistentNaming
        private PropertyInfo __tableSetPropertyInfo;

        private PropertyInfo GetTableSetPropertyInfo()
        {
            if (__tableSetPropertyInfo == null)
            {
                __tableSetPropertyInfo = typeof(DatastoreContext).GetProperty(_settings.TableName);
            }

            return __tableSetPropertyInfo;
        }

        private IEnumerable<Entity> FindAnyMessageEntitiesWithConfiguredExpression(DatastoreContext context)
        {
            var tablePropertyInfo = GetTableSetPropertyInfo();

            if (!(tablePropertyInfo.GetValue(context) is IQueryable<Entity>))
            {
                throw new ConfigurationErrorsException(
                    $"The configured table {_settings.TableName} could not be found");
            }

            // TODO: 
            // - validate the Filter clause for sql injection
            // - make sure that single quotes are used around string vars.  
            //      (Maybe make it dependent on the DB type, same is true for escape characters [] in sql server, ...             

            IEnumerable<Entity> entities = 
                _retrieveEntities == null
                    ? context.NativeCommands.ExclusivelyRetrieveEntities(Table, Filter, TakeRows)
                    : _retrieveEntities(context);

            if (!entities.Any())
            {
                return entities;
            }

            LockEntitiesBeforeContinueToProcessThem(entities);

            context.SaveChanges();

            return entities;
        }

        // TODO: isn't this someting 'Lazy<>' would solve?
        // ReSharper disable InconsistentNaming  (__ indicates that this field should not be used directly; use the GetUpdateFieldProperty instead.)               
        private PropertyInfo __updateProperty;
        // ReSharper restore InconsistentNaming

        private PropertyInfo GetUpdateFieldProperty(Entity entity)
        {
            PropertyInfo FindPropertyInHierarchy(string propertyName, Type t)
            {
                if (t == null)
                {
                    return null;
                }

                PropertyInfo pi = t.GetProperty(
                    propertyName,
                    BindingFlags.Instance
                    | BindingFlags.DeclaredOnly
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);

                if (pi == null)
                {
                    pi = FindPropertyInHierarchy(propertyName, t.BaseType);
                }

                return pi;
            }

            if (__updateProperty == null)
            {
                __updateProperty = FindPropertyInHierarchy(_settings.UpdateField, entity.GetType());

                if (__updateProperty == null)
                {
                    throw new ConfigurationErrorsException(
                        $"The configured Update-field {_settings.UpdateField} could not be found");
                }
            }

            return __updateProperty;
        }

        private void LockEntitiesBeforeContinueToProcessThem(IEnumerable<Entity> entities)
        {
            if (entities.Any() == false)
            {
                return;
            }

            PropertyInfo updateFieldInfo = GetUpdateFieldProperty(entities.First());

            foreach (Entity entity in entities)
            {
                object updateValue = Conversion.Convert(updateFieldInfo.PropertyType, Update);

                Logger.Debug($"Update {entity.GetType().Name}.{updateFieldInfo.Name} to {updateValue}");

                updateFieldInfo.SetValue(
                    obj: entity, 
                    value: updateValue, 
                    invokeAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
                    binder: null, 
                    index: null, 
                    culture: null);
            }
        }

        private async void ReceiveMessageEntity(
            MessageEntity messageEntity,
            Function messageCallback,
            CancellationToken token)
        {
            Logger.Info($"[{messageEntity.EbmsMessageId}] Received Message from {Table} ({Filter})");

            using (Stream stream = await messageEntity.RetrieveMessageBody(Registry.Instance.MessageBodyStore))
            {
                if (stream == null)
                {
                    Logger.Error(
                        $"MessageBody cannot be retrieved for Ebms Message Id: {messageEntity.EbmsMessageId}");
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
            return new ReceivedEntityMessage(messageEntity, stream, messageEntity.ContentType);
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
        private string Table => _settings.TableName;

        [Info("Filter", required: true)]
        private string Filter => _settings.Filter;

        [Info("How many rows to take", defaultValue: SettingKeys.TakeRowsDefault, type: "int32")]
        private int TakeRows => _settings.TakeRows;

        [Info("Update", attributes: new[] { "field" })]
        private string Update => _settings.UpdateValue;

        private static class SettingKeys
        {
            public const string PollingInterval = "PollingInterval";
            public const string Table = "Table";
            public const string Filter = "Filter";
            public const string TakeRows = "BatchSize";
            public const string TakeRowsDefault = "20";
            public const string Update = "Update";
            public const string PollingIntervalDefault = "00:00:03";
        }

        [Info("Polling interval (every)", defaultValue: SettingKeys.PollingIntervalDefault)]
        protected override TimeSpan PollingInterval => _settings.PollingInterval;

        private static TimeSpan GetPollingIntervalFromProperties(IDictionary<string, Setting> properties)
        {
            if (properties.ContainsKey(SettingKeys.PollingInterval) == false)
            {
                return DatastoreReceiverSettings.DefaultPollingInterval;
            }

            var pollingInterval = properties[SettingKeys.PollingInterval];
            return pollingInterval.Value.AsTimeSpan(DatastoreReceiverSettings.DefaultPollingInterval);
        }

        public void Configure(DatastoreReceiverSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        void IReceiver.Configure(IEnumerable<Setting> settings)
        {
            var properties = settings.ToDictionary(s => s.Key, s => s);

            Setting configuredTakeRecords = properties.ReadOptionalProperty(SettingKeys.TakeRows, null);

            if (configuredTakeRecords == null 
                || Int32.TryParse(configuredTakeRecords.Value, out int takeRecords) == false)
            {
                takeRecords = DatastoreReceiverSettings.DefaultTakeRows;
            }

            TimeSpan pollingInterval = GetPollingIntervalFromProperties(properties);

            Setting updateSetting = properties.ReadMandatoryProperty(SettingKeys.Update);

            if (updateSetting["field"] == null)
            {
                throw new ConfigurationErrorsException(
                    "The Update setting does not contain a field attribute that indicates the field that must be updated");
            }

            _settings = new DatastoreReceiverSettings(
                tableName: properties.ReadMandatoryProperty(SettingKeys.Table).Value,
                filter: properties.ReadMandatoryProperty(SettingKeys.Filter).Value,
                updateField: updateSetting["field"].Value,
                updateValue: updateSetting.Value,
                pollingInterval: pollingInterval,
                takeRows: takeRecords);
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
                logger.Error($"Polling on table {Table} with interval {PollingInterval.TotalSeconds} seconds");
                logger.Trace(exception.StackTrace);

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
            if (entity is MessageEntity messageEntity)
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

            if (!(exception is AggregateException aggregate))
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