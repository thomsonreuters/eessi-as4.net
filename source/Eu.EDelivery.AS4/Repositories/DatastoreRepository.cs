using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using NLog;

namespace Eu.EDelivery.AS4.Repositories
{
    /// <summary>
    /// High level repository to use the Data store in a uniform way
    /// </summary>
    public class DatastoreRepository : IDatastoreRepository
    {
        private readonly DatastoreContext _datastoreContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreRepository"/> class. 
        /// Create a high level Repository with a given Data store Context
        /// </summary>
        /// <param name="datastore">
        /// </param>     
        public DatastoreRepository(DatastoreContext datastore)
        {
            _datastoreContext = datastore;
        }

        #region InMessage related functionality

        /// <summary>
        /// Verifies whether there exists an InMessage entity that conforms to the specified predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool InMessageExists(Expression<Func<InMessage, bool>> predicate)
        {
            return _datastoreContext.InMessages.Any(predicate);
        }

        /// <summary>
        /// Selects the in messages.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="messageId"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public TResult GetInMessageData<TResult>(string messageId, Expression<Func<InMessage, TResult>> selection)
        {
            return
                _datastoreContext.InMessages.Where(m => m.EbmsMessageId.Equals(messageId)).Select(selection).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves information for specified InMessages.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="where">The where.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>s
        public IEnumerable<TResult> GetInMessageData<TResult>(Expression<Func<InMessage, bool>> where, Expression<Func<InMessage, TResult>> selection)
        {
            return _datastoreContext.InMessages.Where(where).Select(selection);
        }

        /// <summary>
        /// Retrieves information for a <see cref="InMessage"/> for a given <paramref name="messageId"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result to return</typeparam>
        /// <param name="messageId">The identifier to locate the <see cref="InMessage"/></param>
        /// <param name="selection">The selector function to manipulate the <typeparamref name="TResult"/> type</param>
        /// <returns></returns>
        public TResult GetInMessageData<TResult>(long messageId, Expression<Func<InMessage, TResult>> selection)
        {
            return GetInMessageData(m => m.Id == messageId, selection).SingleOrDefault();
        }

        /// <summary>
        /// Selects some information of specified InMessages.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="messageIds"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        public IEnumerable<TResult> GetInMessagesData<TResult>(IEnumerable<string> messageIds, Expression<Func<InMessage, TResult>> selection)
        {
            if (messageIds.Any() == false)
            {
                return new TResult[] { };
            }

            return _datastoreContext.InMessages.Where(m => messageIds.Contains(m.EbmsMessageId)).Select(selection);
        }

        /// <summary>
        /// Select all the found 'EbmsMessageIds' in the given datastore.
        /// </summary>
        /// <param name="searchedMessageIds">Collection of 'EbmsMessageIds' to be search for.</param>
        /// <returns></returns>
        public IEnumerable<string> SelectExistingInMessageIds(IEnumerable<string> searchedMessageIds)
        {
            return
                _datastoreContext.InMessages.Where(m => searchedMessageIds.Contains(m.EbmsMessageId))
                                            .Select(m => m.EbmsMessageId);
        }

        /// <summary>
        /// Search all the found 'RefToMessageIds' in the given datastore.
        /// </summary>
        /// <param name="searchedMessageIds"></param>
        /// <returns></returns>
        public IEnumerable<string> SelectExistingRefInMessageIds(IEnumerable<string> searchedMessageIds)
        {
            return _datastoreContext.InMessages
                             .Where(m => searchedMessageIds.Contains(m.EbmsRefToMessageId))
                             .Select(m => m.EbmsRefToMessageId);
        }

        /// <summary>
        /// Insert a given <see cref="InMessage"/> into the Data store
        /// </summary>
        /// <param name="inMessage"></param>
        public void InsertInMessage(InMessage inMessage)
        {
            inMessage.InsertionTime = DateTimeOffset.Now;
            inMessage.ModificationTime = DateTimeOffset.Now;

            if (String.IsNullOrWhiteSpace(inMessage.MessageLocation))
            {
                throw new InvalidDataException("InMessage.MessageLocation has not been set.");
            }

            _datastoreContext.InMessages.Add(inMessage);
        }

        /// <summary>
        /// Updates a <see cref="InMessage"/> using a given <paramref name="update"/> function.
        /// </summary>
        /// <param name="id">The identifier to locate the <see cref="InMessage"/></param>
        /// <param name="update">The update function to change the located <see cref="InMessage"/></param>
        public void UpdateInMessage(long id, Action<InMessage> update)
        {
            InMessage entity = _datastoreContext.InMessages.Single(m => m.Id == id);

            update(entity);

            entity.ModificationTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// Update a found InMessage (by AS4 Message Id) in the Data store
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="updateAction"></param>
        public void UpdateInMessage(string messageId, Action<InMessage> updateAction)
        {
            // There might exist multiple InMessage records for the given messageId, therefore we cannot use
            // caching here.            
            long[] inMessageIds = _datastoreContext.InMessages.Where(m => m.EbmsMessageId.Equals(messageId)).Select(m => m.Id).ToArray();

            foreach (long id in inMessageIds)
            {
                InMessage message = GetInMessageEntityFor(messageId, id);
                if (message == null)
                {
                    LogManager.GetCurrentClassLogger().Warn($"Unable to update InMessage {messageId}.  There exists no such InMessage.");
                    return;
                }

                updateAction(message);
                message.ModificationTime = DateTimeOffset.Now;
            }
        }

        /// <summary>
        /// Updates a set of <see cref="InMessage"/> entities using a <paramref name="updateAction"/> function 
        /// for which the given <paramref name="predicate"/> holds.
        /// </summary>
        /// <param name="predicate">The predicate function to locate a set of <see cref="InMessage"/> entities</param>
        /// <param name="updateAction">The update function to change the located <see cref="InMessage"/> entities</param>
        public void UpdateInMessages(Expression<Func<InMessage, bool>> predicate, Action<InMessage> updateAction)
        {
            var inMessageIds = _datastoreContext.InMessages.Where(predicate).Select(m => new { m.EbmsMessageId, m.Id }).ToArray();

            if (inMessageIds.Any())
            {
                foreach (var idSet in inMessageIds)
                {
                    InMessage message = GetInMessageEntityFor(idSet.EbmsMessageId, idSet.Id);
                    if (message != null)
                    {
                        updateAction(message);
                        message.ModificationTime = DateTimeOffset.Now;
                    }
                }
            }
        }

        private InMessage GetInMessageEntityFor(string ebmsMessageId, long id)
        {
            var msg = new InMessage(ebmsMessageId: ebmsMessageId);

            msg.InitializeIdFromDatabase(id);

            if (_datastoreContext.IsEntityAttached(msg) == false)
            {
                _datastoreContext.Attach(msg);
            }
            else
            {
                msg = _datastoreContext.InMessages.FirstOrDefault(m => m.Id == id);
                if (msg == null)
                {
                    LogManager.GetCurrentClassLogger().Error($"No InMessage found for MessageId {ebmsMessageId}");
                    return null;
                }
            }

            return msg;
        }

        #endregion

        #region OutMessage related functionality

        /// <summary>
        /// Determines whether any stored <see cref="OutMessage"/> satisfies a given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate function used in the determination</param>
        /// <returns></returns>
        public bool OutMessageExists(Expression<Func<OutMessage, bool>> predicate)
        {
            return _datastoreContext.OutMessages.Any(predicate);
        }

        /// <summary>
        /// Retrieves information for a single <see cref="OutMessage"/> for a given <paramref name="messageId"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result to return</typeparam>
        /// <param name="messageId">The identifier to locate the <see cref="OutMessage"/></param>
        /// <param name="selection">The selector function to manipulate the <typeparamref name="TResult"/> type</param>
        /// <returns></returns>
        public TResult GetOutMessageData<TResult>(long messageId, Expression<Func<OutMessage, TResult>> selection)
        {
            return GetOutMessageData(m => m.Id == messageId, selection).SingleOrDefault();
        }

        /// <summary>
        /// Retrieves information for specified OutMessages.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="where">The where.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public IEnumerable<TResult> GetOutMessageData<TResult>(Expression<Func<OutMessage, bool>> where, Expression<Func<OutMessage, TResult>> selection)
        {
            return _datastoreContext.OutMessages.Where(where).Select(selection);
        }

        /// <summary>
        /// Insert a given <see cref="OutMessage"/>
        /// into the Data store
        /// </summary>
        /// <param name="outMessage"></param>        
        public void InsertOutMessage(OutMessage outMessage)
        {
            outMessage.InsertionTime = DateTimeOffset.Now;
            outMessage.ModificationTime = DateTimeOffset.Now;

            if (String.IsNullOrWhiteSpace(outMessage.MessageLocation))
            {
                throw new InvalidDataException("OutMessage.MessageLocation has not been set.");
            }

            _datastoreContext.OutMessages.Add(outMessage);
        }

        /// <summary>
        /// Update a found OutMessage (by AS4 Message Id) in the Data store.
        /// </summary>
        /// <param name="outMessageId"></param>
        /// <param name="updateAction"></param>
        public void UpdateOutMessage(long outMessageId, Action<OutMessage> updateAction)
        {
            OutMessage msg = GetOutMessageEntityFor(outMessageId);
            UpdateMessageEntityIfNotNull(updateAction, msg);
        }

        /// <summary>
        /// Updates a set of <see cref="OutMessage"/> entities using a given <paramref name="updateAction"/>
        /// for which the given <paramref name="predicate"/> holds.
        /// </summary>
        /// <param name="predicate">The predicate function to locate the <see cref="OutMessage"/> entities</param>
        /// <param name="updateAction">The update function to change the located <see cref="OutMessage"/> entities</param>
        public void UpdateOutMessages(Expression<Func<OutMessage, bool>> predicate, Action<OutMessage> updateAction)
        {
            var keys = _datastoreContext.OutMessages.Where(predicate).Select(m => new { m.Id }).ToArray();

            foreach (var key in keys)
            {
                var msg = GetOutMessageEntityFor(key.Id);
                UpdateMessageEntityIfNotNull(updateAction, msg);
            }
        }

        private OutMessage GetOutMessageEntityFor(long outMessageId)
        {
            var msg = new OutMessage(null);
            msg.InitializeIdFromDatabase(outMessageId);

            if (_datastoreContext.IsEntityAttached(msg) == false)
            {
                _datastoreContext.Attach(msg);
            }
            else
            {
                msg = _datastoreContext.OutMessages.FirstOrDefault(m => m.Id == outMessageId);
                if (msg == null)
                {
                    LogManager.GetCurrentClassLogger().Error($"No OutMessage found for OutMessageId {outMessageId}");
                    return null;
                }
            }

            return msg;
        }

        private static void UpdateMessageEntityIfNotNull(Action<OutMessage> updateAction, OutMessage msg)
        {
            if (msg != null)
            {
                updateAction(msg);
                msg.ModificationTime = DateTimeOffset.Now;
            }
        }

        #endregion

        #region OutException related functionality

        /// <summary>
        /// Retrieves information for a single <see cref="OutException"/> entity for a given <paramref name="id"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result to return</typeparam>
        /// <param name="id">The identifier to locate the <see cref="OutException"/> entity</param>
        /// <param name="selector">The selector function to manipulate the <typeparamref name="TResult"/> type</param>
        /// <returns></returns>s
        public TResult GetOutExceptionData<TResult>(long id, Expression<Func<OutException, TResult>> selector)
        {
            return _datastoreContext.OutExceptions.Where(ex => ex.Id == id).Select(selector).SingleOrDefault();
        }

        /// <summary>
        /// Retrieves information for a specified OutException using a <paramref name="refToMessageId"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="refToMessageId"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public IEnumerable<TResult> GetOutExceptionsData<TResult>(
            string refToMessageId,
            Expression<Func<OutException, TResult>> selection)
        {
            return _datastoreContext.OutExceptions.Where(ex => ex.EbmsRefToMessageId == refToMessageId).Select(selection);
        }

        /// <summary>
        /// Insert a given <see cref="OutException"/> into the Data store.
        /// </summary>
        /// <param name="outException"></param>
        public void InsertOutException(OutException outException)
        {
            outException.InsertionTime = DateTimeOffset.Now;
            outException.ModificationTime = DateTimeOffset.Now;

            _datastoreContext.OutExceptions.Add(outException);
        }

        /// <summary>
        /// Updates a single <see cref="OutException"/> entity for given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier to locate the <see cref="OutException"/> entity</param>
        /// <param name="update">The update function to change the located <see cref="OutException"/> entity</param>
        public void UpdateOutException(long id, Action<OutException> update)
        {
            OutException entity = _datastoreContext.OutExceptions.Single(ex => ex.Id == id);

            update(entity);

            entity.ModificationTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// Update a found OutException (by AS4 Ref Message Id) in the Data store.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        public void UpdateOutException(string refToMessageId, Action<OutException> updateAction)
        {

            IEnumerable<OutException> outExceptions = _datastoreContext.OutExceptions
                .Where(m => m.EbmsRefToMessageId.Equals(refToMessageId));

            foreach (OutException outException in outExceptions)
            {
                updateAction(outException);
                outException.ModificationTime = DateTimeOffset.Now;
            }
        }


        #endregion

        #region InException functionality

        /// <summary>
        /// Retrieves information for a single <see cref="InException"/> for a given <paramref name="id"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result to return</typeparam>
        /// <param name="id">The identifier to locate the <see cref="InException"/></param>
        /// <param name="selector">The selector function to manipulate the <typeparamref name="TResult"/> type</param>
        /// <returns></returns>
        public TResult GetInExceptionData<TResult>(long id, Expression<Func<InException, TResult>> selector)
        {
            return _datastoreContext.InExceptions.Where(ex => ex.Id == id).Select(selector).SingleOrDefault();
        }

        /// <summary>
        /// Retrieves information for specified InException.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="refToMessageId"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public IEnumerable<TResult> GetInExceptionsData<TResult>(
            string refToMessageId,
            Expression<Func<InException, TResult>> selection)
        {
            return _datastoreContext.InExceptions.Where(ex => ex.EbmsRefToMessageId == refToMessageId).Select(selection);
        }

        /// <summary>
        /// Insert a given <see cref="InException"/> into the Data store.</summary>
        /// <param name="inException"></param>
        public void InsertInException(InException inException)
        {
            inException.ModificationTime = DateTimeOffset.Now;
            inException.InsertionTime = DateTimeOffset.Now;

            _datastoreContext.InExceptions.Add(inException);
        }

        /// <summary>
        /// Updates a single <see cref="InException"/> for a given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier to locate the <see cref="InException"/> entity</param>
        /// <param name="update">The update function to change the <see cref="InException"/> entity</param>
        public void UpdateInException(long id, Action<InException> update)
        {
            InException entity = _datastoreContext.InExceptions.Single(ex => ex.Id == id);

            update(entity);

            entity.ModificationTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// Update a found InException (by AS4 Ref Message Id) in the Data store.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        public void UpdateInException(string refToMessageId, Action<InException> updateAction)
        {
            IEnumerable<InException> inExceptions = _datastoreContext.InExceptions
                .Where(m => m.EbmsRefToMessageId.Equals(refToMessageId));

            foreach (InException inException in inExceptions)
            {
                updateAction(inException);
                inException.ModificationTime = DateTimeOffset.Now;
            }
        }

        #endregion

        #region RetryReliability related functionality

        /// <summary>
        /// Gets a sequence of <see cref="RetryReliability"/> records based on a given <paramref name="predicate"/>,
        /// using a <paramref name="selector"/> to manipulate to a <typeparamref name="TResult"/> type.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public IEnumerable<TResult> GetRetryReliability<TResult>(
            Expression<Func<RetryReliability, bool>> predicate,
            Expression<Func<RetryReliability, TResult>> selector)
        {
            return _datastoreContext.RetryReliability.Where(predicate).Select(selector);
        }

        /// <summary>
        /// Inserts the retry reliability information referencing a <see cref="InMessage"/>.
        /// </summary>
        /// <param name="reliability">The <see cref="RetryReliability"/> entity to insert</param>
        public void InsertRetryReliability(RetryReliability reliability)
        {
            reliability.InsertionTime = DateTimeOffset.Now;
            reliability.ModificationTime = DateTimeOffset.Now;

            _datastoreContext.RetryReliability.Add(reliability);
        }

        /// <summary>
        /// Inserts the retry reliability informations referencing <see cref="InMessage"/>'s.
        /// </summary>
        /// <param name="reliabilities">The <see cref="RetryReliability"/> entities to insert</param>
        public void InsertRetryReliabilities(IEnumerable<RetryReliability> reliabilities)
        {
            foreach (var r in reliabilities)
            {
                r.InsertionTime = DateTimeOffset.Now;
                r.ModificationTime = DateTimeOffset.Now;
            }

            _datastoreContext.RetryReliability.AddRange(reliabilities);
        }

        /// <summary>
        /// Updates a single <see cref="RetryReliability"/> record for a given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier to locate the <see cref="RetryReliability"/> record</param>
        /// <param name="update">The update function to change the <see cref="RetryReliability"/> record</param>
        public void UpdateRetryReliability(long id, Action<RetryReliability> update)
        {
            RetryReliability rr = _datastoreContext.RetryReliability.SingleOrDefault(r => r.Id == id);
            if (rr != null)
            {
                update(rr);
                rr.ModificationTime = DateTimeOffset.Now;
            }
        }

        #endregion
    }
}