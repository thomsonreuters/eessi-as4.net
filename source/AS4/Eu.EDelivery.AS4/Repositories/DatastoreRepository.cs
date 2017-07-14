using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using ReceptionAwareness = Eu.EDelivery.AS4.Entities.ReceptionAwareness;

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
        public bool InMessageExists(Func<InMessage, bool> predicate)
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
        public TResult GetInMessageData<TResult>(string messageId, Func<InMessage, TResult> selection)
        {
            return
                _datastoreContext.InMessages.Where(m => m.EbmsMessageId.Equals(messageId)).Select(selection).FirstOrDefault();
        }

        /// <summary>
        /// Selects some information of specified InMessages.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="messageIds"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        public IEnumerable<TResult> GetInMessagesData<TResult>(IEnumerable<string> messageIds, Func<InMessage, TResult> selection)
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
            if (String.IsNullOrWhiteSpace(inMessage.MessageLocation))
            {
                // TODO: throw an exception.
            }

            _datastoreContext.InMessages.Add(inMessage);
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

        private InMessage GetInMessageEntityFor(string ebmsMessageId, long id)
        {
            var msg = new InMessage { EbmsMessageId = ebmsMessageId };

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
        /// Firsts the or default out message.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public TResult GetOutMessageData<TResult>(string messageId, Func<OutMessage, TResult> selection)
        {
            return GetOutMessageData(m => m.EbmsMessageId.Equals(messageId), selection);
        }

        /// <summary>
        /// Gets the out message data.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="where">The where.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public TResult GetOutMessageData<TResult>(Func<OutMessage, bool> where, Func<OutMessage, TResult> selection)
        {
            return _datastoreContext.OutMessages.Where(where).Select(selection).FirstOrDefault();
        }

        /// <summary>
        /// Insert a given <see cref="OutMessage"/>
        /// into the Data store
        /// </summary>
        /// <param name="outMessage"></param>        
        public void InsertOutMessage(OutMessage outMessage)
        {
            if (String.IsNullOrWhiteSpace(outMessage.MessageLocation))
            {
                // TODO: throw exception.
            }

            _datastoreContext.OutMessages.Add(outMessage);


        }

        /// <summary>
        /// Update a found OutMessage (by AS4 Message Id) in the Data store.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="updateAction"></param>
        public void UpdateOutMessage(string messageId, Action<OutMessage> updateAction)
        {
            // We need to know the Id, since Attaching only works when the Primary Key is known.
            // Still, retrieving only the PK will still be faster then retrieving the complete entity.
            // Maybe we should define the ebmsId as the PK ?            
            Dictionary<string, long> keyMap = GetMessageIdsForEbmsMessageIds(_datastoreContext.OutMessages, _outMessageIdMap, messageId);

            if (keyMap.ContainsKey(messageId))
            {
                OutMessage msg = GetOutMessageEntityFor(messageId, keyMap[messageId]);
                UpdateMessageEntityIfNotNull(updateAction, msg);
            }
        }

        public void UpdateOutMessages(IEnumerable<string> messageIds, Action<OutMessage> updateAction)
        {
            Dictionary<string, long> idMap = GetMessageIdsForEbmsMessageIds(_datastoreContext.OutMessages, _outMessageIdMap, messageIds.ToArray());

            foreach (KeyValuePair<string, long> kvp in idMap)
            {
                OutMessage msg = GetOutMessageEntityFor(kvp.Key, kvp.Value);
                UpdateMessageEntityIfNotNull(updateAction, msg);
            }
        }

        private OutMessage GetOutMessageEntityFor(string ebmsMessageId, long id)
        {
            var msg = new OutMessage { EbmsMessageId = ebmsMessageId };
            msg.InitializeIdFromDatabase(id);

            if (_datastoreContext.IsEntityAttached(msg) == false)
            {
                _datastoreContext.Attach(msg);
            }
            else
            {
                msg = _datastoreContext.OutMessages.FirstOrDefault(m => m.EbmsMessageId == ebmsMessageId);
                if (msg == null)
                {
                    LogManager.GetCurrentClassLogger().Error($"No OutMessage found for MessageId {ebmsMessageId}");
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

        #region Reception Awareness related functionality

        /// <summary>
        /// Insert a given <see cref="ReceptionAwareness"/> into the Data store.
        /// </summary>
        /// <param name="receptionAwareness"></param>
        public void InsertReceptionAwareness(ReceptionAwareness receptionAwareness)
        {
            _datastoreContext.ReceptionAwareness.Add(receptionAwareness);
        }

        public IEnumerable<ReceptionAwareness> GetReceptionAwareness(IEnumerable<string> messageIds)
        {
            ReceptionAwareness[] entities = _datastoreContext.ReceptionAwareness.Where(r => messageIds.Contains(r.InternalMessageId)).ToArray();

            foreach (ReceptionAwareness entity in entities)
            {
                _receptionAwarenessIdMap.Set(entity.InternalMessageId, entity.Id, CacheLifeTime);
            }

            return entities;
        }

        /// <summary>
        /// Update a found <see cref="ReceptionAwareness"/> in the Data store.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        public void UpdateReceptionAwareness(string refToMessageId, Action<ReceptionAwareness> updateAction)
        {
            long id = GetReceptionAwarenessIdForMessageId(_datastoreContext, refToMessageId);

            if (id == default(long))
            {
                LogManager.GetCurrentClassLogger().Error($"Unable to update ReceptionAwareness entity. No record exists for MessageId {refToMessageId}");
                return;
            }

            var entity = new ReceptionAwareness { InternalMessageId = refToMessageId };
            entity.InitializeIdFromDatabase(id);

            if (_datastoreContext.IsEntityAttached(entity) == false)
            {
                _datastoreContext.Attach(entity);
            }
            else
            {
                entity = _datastoreContext.ReceptionAwareness.FirstOrDefault(r => r.Id == id);

                if (entity == null)
                {
                    LogManager.GetCurrentClassLogger().Error($"Unable to update ReceptionAwareness entity. No record exists for MessageId {refToMessageId}");
                    return;
                }
            }

            updateAction(entity);
        }

        #endregion

        #region OutException related functionality

        /// <summary>
        /// Insert a given <see cref="OutException"/> into the Data store.
        /// </summary>
        /// <param name="outException"></param>
        public void InsertOutException(OutException outException)
        {
            _datastoreContext.OutExceptions.Add(outException);
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
        /// Insert a given <see cref="InException"/> into the Data store.</summary>
        /// <param name="inException"></param>
        public void InsertInException(InException inException)
        {
            _datastoreContext.InExceptions.Add(inException);
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

        #region MessageId <> Id mapping

        // TODO: encapsulate in an inner class which implements IDisposable.
        private static MemoryCache _outMessageIdMap = new MemoryCache(new MemoryCacheOptions());
        private static MemoryCache _receptionAwarenessIdMap = new MemoryCache(new MemoryCacheOptions());

        private static readonly TimeSpan CacheLifeTime = TimeSpan.FromSeconds(30);

        internal static void ResetCaches()
        {
            _outMessageIdMap = new MemoryCache(new MemoryCacheOptions());
            _receptionAwarenessIdMap = new MemoryCache(new MemoryCacheOptions());
        }

        public static void DisposeCaches()
        {
            _outMessageIdMap.Dispose();
            _receptionAwarenessIdMap.Dispose();
        }

        private static Dictionary<string, long> GetMessageIdsForEbmsMessageIds<T>(IQueryable<T> messages, IMemoryCache cache, params string[] ebmsMessageIds) where T : MessageEntity
        {
            var result = new Dictionary<string, long>();

            var nonCachedMessageIds = new HashSet<string>();

            ebmsMessageIds = ebmsMessageIds.Distinct().ToArray();

            // First try to retrieve the items that are already cached.
            foreach (string messageId in ebmsMessageIds)
            {
                if (cache.TryGetValue(messageId, out long id))
                {
                    result.Add(messageId, id);
                }
                else
                {
                    nonCachedMessageIds.Add(messageId);
                }
            }

            if (nonCachedMessageIds.Any())
            {
                var map = messages.Where(m => nonCachedMessageIds.Contains(m.EbmsMessageId)).Select(m => new { m.EbmsMessageId, m.Id });

                foreach (var item in map)
                {
                    cache.Set(item.EbmsMessageId, item.Id, CacheLifeTime);
                    if (result.ContainsKey(item.EbmsMessageId) == false)
                    {
                        result.Add(item.EbmsMessageId, item.Id);
                    }
                }
            }

            return result;
        }

        private static long GetReceptionAwarenessIdForMessageId(DatastoreContext context, string messageId)
        {

            if (_receptionAwarenessIdMap.TryGetValue(messageId, out long id) == false)
            {
                id = context.ReceptionAwareness.Where(r => r.InternalMessageId == messageId).Select(r => r.Id).FirstOrDefault();

                if (id != default(long))
                {
                    _receptionAwarenessIdMap.Set(messageId, id, CacheLifeTime);
                }
            }

            return id;
        }

        #endregion
    }
}