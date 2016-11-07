using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

using Microsoft.EntityFrameworkCore;
using NLog;
using Polly;
using Polly.Retry;

namespace Eu.EDelivery.AS4.Repositories
{
    /// <summary>
    /// High level repository to use the Data store in a uniform way
    /// </summary>
    public class DatastoreRepository : IDatastoreRepository
    {
        private readonly ILogger _logger;
        private readonly Func<DatastoreContext> _datastore;
        private RetryPolicy _policy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreRepository"/> class. 
        /// Create a high level Repository
        /// with a given Data store Context
        /// </summary>
        /// <param name="datastore">
        /// </param>
        public DatastoreRepository(Func<DatastoreContext> datastore)
        {
            this._datastore = datastore;
            this._policy = Policy.Handle<DbUpdateException>().RetryAsync();
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Get a <see cref="InMessage"/>
        /// for a given AS4 Message Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public InMessage GetInMessageById(string messageId)
        {
            using (DatastoreContext context = this._datastore())
                return context.InMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
        }

        public InMessage GetInMessage(Func<InMessage, bool> predicate)
        {
            using (DatastoreContext context = this._datastore())
                return context.InMessages.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Get a <see cref="InMessage"/>
        /// for a given AS4 Message Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public OutMessage GetOutMessageById(string messageId)
        {
            using (DatastoreContext context = this._datastore())
                return context.OutMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
        }

        /// <summary>
        /// Get a <see cref="ReceptionAwareness"/>
        /// for a given AS4 Message Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public ReceptionAwareness GetReceptionAwareness(string messageId)
        {
            using (DatastoreContext context = this._datastore())
                return context.ReceptionAwareness.FirstOrDefault(a => a.InternalMessageId.Equals(messageId));
        }

        /// <summary>
        /// Insert a given <see cref="OutMessage"/>
        /// into the Data store
        /// </summary>
        /// <param name="outMessage"></param>
        public async Task InsertOutMessageAsync(OutMessage outMessage)
        {
            using (DatastoreContext context = this._datastore())
            {
                context.OutMessages.Add(outMessage);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Insert a given <see cref="OutException"/>
        /// into the Data store
        /// </summary>
        /// <param name="outException"></param>
        /// <returns></returns>
        public async Task InsertOutExceptionAsync(OutException outException)
        {
            using (DatastoreContext context = this._datastore())
            {
                context.OutExceptions.Add(outException);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Insert a given <see cref="InMessage"/>
        /// into the Data store
        /// </summary>
        /// <param name="inMessage"></param>
        /// <returns></returns>
        public async Task InsertInMessageAsync(InMessage inMessage)
        {
            using (DatastoreContext context = this._datastore())
            {
                context.InMessages.Add(inMessage);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Insert a given <see cref="InException"/>
        /// into the Data store
        /// </summary>
        /// <param name="inException"></param>
        /// <returns></returns>
        public async Task InsertInExceptionAsync(InException inException)
        {
            using (DatastoreContext context = this._datastore())
            {
                context.InExceptions.Add(inException);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Insert a given <see cref="ReceptionAwareness"/>
        /// into the Data store
        /// </summary>
        /// <param name="receptionAwareness"></param>
        /// <returns></returns>
        public async Task InsertReceptionAwarenessAsync(ReceptionAwareness receptionAwareness)
        {
            using (DatastoreContext context = this._datastore())
            {
                context.ReceptionAwareness.Add(receptionAwareness);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Update a found OutMessage (by AS4 Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateOutMessage(string messageId, Action<OutMessage> updateAction)
        {
            using (DatastoreContext context = this._datastore())
            {
                OutMessage outMessage = context.OutMessages
                    .FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));

                if (outMessage == null) return;
                updateAction(outMessage);
                outMessage.ModificationTime = DateTimeOffset.UtcNow;
                context.Update(outMessage);

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Update a found InMessage (by AS4 Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateInMessageAsync(string messageId, Action<InMessage> updateAction)
        {
            using (DatastoreContext context = this._datastore())
            {
                InMessage inMessage = context.InMessages
                    .FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));

                if (inMessage == null) return;
                updateAction(inMessage);
                inMessage.ModificationTime = DateTimeOffset.UtcNow;
                context.Update(inMessage);

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Update a found InException (by AS4 Ref Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateInExceptionAsync(string refToMessageId, Action<InException> updateAction)
        {
            using (DatastoreContext context = this._datastore())
            {
                IEnumerable<InException> inExceptions = context.InExceptions
                    .Where(m => m.EbmsRefToMessageId.Equals(refToMessageId));

                foreach (InException inException in inExceptions)
                {
                    updateAction(inException);
                    inException.ModificationTime = DateTimeOffset.UtcNow;
                    context.Update(inException);

                    await context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Update a found OutException (by AS4 Ref Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateOutExceptionAsync(string refToMessageId, Action<OutException> updateAction)
        {
            using (DatastoreContext context = this._datastore())
            {
                IEnumerable<OutException> outExceptions = context.OutExceptions
                    .Where(m => m.EbmsRefToMessageId.Equals(refToMessageId));

                foreach (OutException outException in outExceptions)
                {
                    updateAction(outException);
                    outException.ModificationTime = DateTimeOffset.UtcNow;
                    context.Update(outException);

                    await context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Update a found <see cref="ReceptionAwareness"/>
        /// in the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public async Task UpdateReceptionAwareness(string refToMessageId, Action<ReceptionAwareness> updateAction)
        {
            using (DatastoreContext context = this._datastore())
            {
                ReceptionAwareness receptionAwareness = context.ReceptionAwareness
                    .FirstOrDefault(a => a.InternalMessageId.Equals(refToMessageId));

                if (receptionAwareness == null) return;
                updateAction(receptionAwareness);
                context.Update(receptionAwareness);

                await context.SaveChangesAsync();
            }
        }
    }

    public interface IDatastoreRepository
    {
        OutMessage GetOutMessageById(string messageId);
        InMessage GetInMessageById(string messageId);
        ReceptionAwareness GetReceptionAwareness(string messageId);

        Task InsertInExceptionAsync(InException inException);
        Task InsertInMessageAsync(InMessage inMessage);
        Task InsertOutExceptionAsync(OutException outException);
        Task InsertOutMessageAsync(OutMessage outMessage);
        Task InsertReceptionAwarenessAsync(ReceptionAwareness receptionAwareness);

        Task UpdateInMessageAsync(string messageId, Action<InMessage> updateAction);
        Task UpdateOutMessage(string messageId, Action<OutMessage> updateAction);
        Task UpdateInExceptionAsync(string refToMessageId, Action<InException> updateAction);
        Task UpdateOutExceptionAsync(string refToMessageId, Action<OutException> updateAction);
        Task UpdateReceptionAwareness(string messageId, Action<ReceptionAwareness> updateAction);

        InMessage GetInMessage(Func<InMessage, bool> predicate);
        
    }
}