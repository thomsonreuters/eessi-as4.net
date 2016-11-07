using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers.Specifications;
using NLog;

using Expression = System.Linq.Expressions.Expression<System.Func<
    Eu.EDelivery.AS4.Common.DatastoreContext,
    System.Collections.Generic.IEnumerable<Eu.EDelivery.AS4.Entities.Entity>>>;

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

        protected override TimeSpan PollingInterval { get; }
        protected override ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiver"/> class
        /// </summary>
        public DatastoreReceiver()
        {
            this._specification = new DatastoreSpecification();
            this.PollingInterval = TimeSpan.FromSeconds(1);
            this.Logger = LogManager.GetCurrentClassLogger();
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
            this._storeExpression = storeExpression;
            this._findExpression = findExpression;
            this._operation = updatedOperation;

            this._specification = new DatastoreSpecification();
            this.PollingInterval = TimeSpan.FromSeconds(1);
            this.Logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Configure the receiver with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        public void Configure(IDictionary<string, string> properties)
        {
            this._properties = properties;
            this._specification.Configure(properties);
            this._findExpression = this._specification.GetExpression().Compile();
            this._storeExpression = () => new DatastoreContext(Config.Instance);

            if (properties.ContainsKey("Update"))
                this._operation = (Operation)Enum.Parse(typeof(Operation), properties["Update"]);
            else this._operation = Operation.NotApplicable;
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

        private void LogReceiverSpecs()
        {
            if (this._properties == null) return;

            string table = this._properties["Table"];
            string field = this._properties["Field"];
            string value = this._properties["Value"];

            this.Logger.Info($"Start Receiving on Datastore FROM {table} WHERE {field} == {value}");
        }

        /// <summary>
        /// Get the Out Messages from the Store with <see cref="Operation.ToBeSent" /> as Operation
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override IEnumerable<Entity> GetMessagesToPoll(CancellationToken cancellationToken)
        {
            IEnumerable<Entity> entities;
            using (DatastoreContext context = this._storeExpression())
                entities = this._findExpression(context).ToList();

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
            if (messageEntity != null) ReceiveMessageEntity(messageEntity, messageCallback, token);
            else ReveiveEntity(entity, messageCallback, token);
        }

        private void ReceiveMessageEntity(MessageEntity messageEntity, Function messageCallback, CancellationToken token)
        {
            this.Logger.Info($"Received Message from Datastore with Ebms Message Id: {messageEntity.EbmsMessageId}");

            using (var memoryStream = new MemoryStream(messageEntity.MessageBody))
            using (DatastoreContext context = this._storeExpression())
            {
                context.Update(messageEntity);

                if (this._operation != Operation.NotApplicable)
                    messageEntity.Operation = this._operation;

                context.SaveChanges();

                ReceivedMessage receivedMessage = CreateReceivedMessage(messageEntity, memoryStream);
                messageCallback(receivedMessage, token);
            }
        }

        private ReceivedMessage CreateReceivedMessage(MessageEntity messageEntity, Stream stream)
        {
            var entityMessage = new ReceivedMessageEntityMessage(messageEntity: messageEntity);
            entityMessage.RequestStream = stream;
            entityMessage.ContentType = messageEntity.ContentType;

            return entityMessage;
        }

        private void ReveiveEntity(Entity entity, Function messageCallback, CancellationToken token)
        {
            var message = new ReceivedEntityMessage(entity);
            messageCallback(message, token);
        }

        /// <summary>
        /// Describe what to do when a
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected override void HandleMessageException(Entity message, Exception exception)
        {
            this.Logger.Error(exception.Message);
        }
    }

}