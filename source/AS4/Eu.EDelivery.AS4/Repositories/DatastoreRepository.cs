using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Microsoft.EntityFrameworkCore;
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
        private readonly DatastoreContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreRepository"/> class. 
        /// Create a high level Repository with a given Data store Context
        /// </summary>
        /// <param name="datastore">
        /// </param>     
        public DatastoreRepository(DatastoreContext datastore)
        {
            _dbContext = datastore;
        }

        #region InMessage related functionality

        /// <summary>
        /// Get a <see cref="InMessage"/>
        /// for a given AS4 Message Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public InMessage GetInMessageById(string messageId)
        {
            var entity = _dbContext.InMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));

            if (entity != null && entity.Id != default(long))
            {
                _inMessageIdMap.Set(messageId, entity.Id, CacheLifeTime);
            }

            return entity;
        }

        public InMessage GetInMessage(Func<InMessage, bool> predicate)
        {
            return _dbContext.InMessages.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Insert a given <see cref="InMessage"/>
        /// into the Data store
        /// </summary>
        /// <param name="inMessage"></param>
        /// <returns></returns>
        public void InsertInMessage(InMessage inMessage)
        {
            _dbContext.InMessages.Add(inMessage);
        }

        /// <summary>
        /// Update a found InMessage (by AS4 Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public void UpdateInMessage(string messageId, Action<InMessage> updateAction)
        {
            var keyMap = GetMessageIdsForEbmsMessageIds(_dbContext.InMessages, _inMessageIdMap, messageId);

            if (keyMap.ContainsKey(messageId))
            {
                var msg = GetInMessageEntityFor(messageId, keyMap[messageId]);
                if (msg == null)
                {
                    LogManager.GetCurrentClassLogger().Warn($"Unable to update InMessage {messageId}.  There exists no such InMessage.");
                    return;
                }

                updateAction(msg);
                msg.ModificationTime = DateTimeOffset.UtcNow;
            }
        }

        private InMessage GetInMessageEntityFor(string ebmsMessageId, long id)
        {
            InMessage msg = new InMessage() { EbmsMessageId = ebmsMessageId };

            msg.InitializeIdFromDatabase(id);

            if (_dbContext.IsEntityAttached(msg) == false)
            {
                _dbContext.Attach(msg);
            }
            else
            {
                msg = _dbContext.InMessages.FirstOrDefault(m => m.EbmsMessageId == ebmsMessageId);
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
        /// Get a <see cref="OutMessage"/> for a given AS4 Message Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public OutMessage GetOutMessageById(string messageId)
        {
            var message = _dbContext.OutMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));

            if (message != null && message.Id != default(long))
            {
                _outMessageIdMap.Set(messageId, message.Id, CacheLifeTime);
            }

            return message;
        }

        /// <summary>
        /// Insert a given <see cref="OutMessage"/>
        /// into the Data store
        /// </summary>
        /// <param name="outMessage"></param>
        public void InsertOutMessage(OutMessage outMessage)
        {
            _dbContext.OutMessages.Add(outMessage);
        }

        /// <summary>
        /// Update a found OutMessage (by AS4 Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public void UpdateOutMessage(string messageId, Action<OutMessage> updateAction)
        {
            // We need to know the Id, since Attaching only works when the Primary Key is known.
            // Still, retrieving only the PK will still be faster then retrieving the complete entity.
            // Maybe we should define the ebmsId as the PK ?            
            var keyMap = GetMessageIdsForEbmsMessageIds(_dbContext.OutMessages, _outMessageIdMap, messageId);

            if (keyMap.ContainsKey(messageId))
            {
                var msg = GetOutMessageEntityFor(messageId, keyMap[messageId]);
                if (msg != null)
                {
                    updateAction(msg);
                    msg.ModificationTime = DateTimeOffset.UtcNow;
                }
            }
        }

        public void UpdateOutMessages(IEnumerable<string> messageIds, Action<OutMessage> updateAction)
        {
            var idMap = GetMessageIdsForEbmsMessageIds(_dbContext.OutMessages, _outMessageIdMap, messageIds.ToArray());

            foreach (var kvp in idMap)
            {
                OutMessage msg = GetOutMessageEntityFor(kvp.Key, kvp.Value);

                if (msg != null)
                {
                    updateAction(msg);
                    msg.ModificationTime = DateTimeOffset.UtcNow;
                }
            }
        }

        private OutMessage GetOutMessageEntityFor(string ebmsMessageId, long id)
        {
            OutMessage msg = new OutMessage() { EbmsMessageId = ebmsMessageId };
            msg.InitializeIdFromDatabase(id);

            if (_dbContext.IsEntityAttached(msg) == false)
            {
                _dbContext.Attach(msg);
            }
            else
            {
                msg = _dbContext.OutMessages.FirstOrDefault(m => m.EbmsMessageId == ebmsMessageId);
                if (msg == null)
                {
                    LogManager.GetCurrentClassLogger().Error($"No OutMessage found for MessageId {ebmsMessageId}");
                    return null;
                }
            }

            return msg;
        }

        public SendingProcessingMode RetrieveSendingPModeForOutMessage(string ebmsMessageId)
        {
            var p = _dbContext.OutMessages.Where(m => m.EbmsMessageId.Equals(ebmsMessageId)).Select(m => m.PMode).FirstOrDefault();
            var pmode = AS4XmlSerializer.FromString<SendingProcessingMode>(p);

            return pmode;
        }

        #endregion

        #region Reception Awareness related functionality

        /// <summary>
        /// Insert a given <see cref="ReceptionAwareness"/>
        /// into the Data store
        /// </summary>
        /// <param name="receptionAwareness"></param>
        /// <returns></returns>
        public void InsertReceptionAwareness(ReceptionAwareness receptionAwareness)
        {
            _dbContext.ReceptionAwareness.Add(receptionAwareness);
        }

        public IEnumerable<ReceptionAwareness> GetReceptionAwareness(IEnumerable<string> messageIds)
        {
            var entities = _dbContext.ReceptionAwareness.Where(r => messageIds.Contains(r.InternalMessageId)).ToArray();

            foreach (var entity in entities)
            {
                _receptionAwarenessIdMap.Set(entity.InternalMessageId, entity.Id, CacheLifeTime);
            }

            return entities;
        }

        /// <summary>
        /// Update a found <see cref="ReceptionAwareness"/>
        /// in the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public void UpdateReceptionAwareness(string refToMessageId, Action<ReceptionAwareness> updateAction)
        {
            long id = GetReceptionAwarenessIdForMessageId(_dbContext, refToMessageId);

            if (id == default(long))
            {
                LogManager.GetCurrentClassLogger().Error($"Unable to update ReceptionAwareness entity. No record exists for MessageId {refToMessageId}");
                return;
            }

            ReceptionAwareness entity = new ReceptionAwareness() { InternalMessageId = refToMessageId };
            entity.InitializeIdFromDatabase(id);

            if (_dbContext.IsEntityAttached(entity) == false)
            {
                _dbContext.Attach(entity);
            }
            else
            {
                entity = _dbContext.ReceptionAwareness.FirstOrDefault(r => r.Id == id);

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
        /// Insert a given <see cref="OutException"/>
        /// into the Data store
        /// </summary>
        /// <param name="outException"></param>
        /// <returns></returns>
        public void InsertOutException(OutException outException)
        {
            _dbContext.OutExceptions.Add(outException);
        }

        /// <summary>
        /// Update a found OutException (by AS4 Ref Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public void UpdateOutException(string refToMessageId, Action<OutException> updateAction)
        {

            IEnumerable<OutException> outExceptions = _dbContext.OutExceptions
                .Where(m => m.EbmsRefToMessageId.Equals(refToMessageId));

            foreach (OutException outException in outExceptions)
            {
                updateAction(outException);
                outException.ModificationTime = DateTimeOffset.UtcNow;
            }
        }


        #endregion

        #region InException functionality

        /// <summary>
        /// Insert a given <see cref="InException"/>
        /// into the Data store
        /// </summary>
        /// <param name="inException"></param>
        /// <returns></returns>
        public void InsertInException(InException inException)
        {
            _dbContext.InExceptions.Add(inException);
        }

        /// <summary>
        /// Update a found InException (by AS4 Ref Message Id)
        /// in the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public void UpdateInException(string refToMessageId, Action<InException> updateAction)
        {
            IEnumerable<InException> inExceptions = _dbContext.InExceptions
                .Where(m => m.EbmsRefToMessageId.Equals(refToMessageId));

            foreach (InException inException in inExceptions)
            {
                updateAction(inException);
                inException.ModificationTime = DateTimeOffset.UtcNow;
            }
        }

        #endregion

        #region MessageId <> Id mapping

        // TODO: encapsulate in an inner class which implements IDisposable.

        private static MemoryCache _outMessageIdMap = new MemoryCache(new MemoryCacheOptions());
        private static MemoryCache _inMessageIdMap = new MemoryCache(new MemoryCacheOptions());
        private static MemoryCache _receptionAwarenessIdMap = new MemoryCache(new MemoryCacheOptions());

        private static readonly TimeSpan CacheLifeTime = TimeSpan.FromSeconds(30);


        internal static void ResetCaches()
        {
            _outMessageIdMap = new MemoryCache(new MemoryCacheOptions());
            _inMessageIdMap = new MemoryCache(new MemoryCacheOptions());
            _receptionAwarenessIdMap = new MemoryCache(new MemoryCacheOptions());
        }

        public static void DisposeCaches()
        {
            _outMessageIdMap.Dispose();
            _inMessageIdMap.Dispose();
            _receptionAwarenessIdMap.Dispose();
        }

        private static Dictionary<string, long> GetMessageIdsForEbmsMessageIds<T>(DbSet<T> messages, MemoryCache cache, params string[] ebmsMessageIds) where T : MessageEntity
        {
            Dictionary<string, long> result = new Dictionary<string, long>();

            HashSet<string> nonCachedMessageIds = new HashSet<string>();

            ebmsMessageIds = ebmsMessageIds.Distinct().ToArray();

            // First try to retrieve the items that are already cached.
            foreach (string messageId in ebmsMessageIds)
            {
                long id;
                if (cache.TryGetValue(messageId, out id))
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
            long id;

            if (_receptionAwarenessIdMap.TryGetValue(messageId, out id) == false)
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

    public interface IDatastoreRepository
    {
        void InsertInMessage(InMessage inMessage);
        void UpdateInMessage(string messageId, Action<InMessage> updateAction);
        InMessage GetInMessage(Func<InMessage, bool> predicate);
        InMessage GetInMessageById(string messageId);

        void InsertOutMessage(OutMessage outMessage);
        void UpdateOutMessage(string messageId, Action<OutMessage> updateAction);
        OutMessage GetOutMessageById(string messageId);

        SendingProcessingMode RetrieveSendingPModeForOutMessage(string ebmsMessageId);

        void InsertReceptionAwareness(ReceptionAwareness receptionAwareness);
        void UpdateReceptionAwareness(string messageId, Action<ReceptionAwareness> updateAction);
        IEnumerable<ReceptionAwareness> GetReceptionAwareness(IEnumerable<string> messageIds);

        void InsertInException(InException inException);
        void UpdateInException(string refToMessageId, Action<InException> updateAction);

        void InsertOutException(OutException outException);
        void UpdateOutException(string refToMessageId, Action<OutException> updateAction);
    }
}