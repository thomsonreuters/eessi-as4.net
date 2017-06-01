using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Repositories
{
    public interface IDatastoreRepository
    {
        /// <summary>
        /// Ins the message exists.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        bool InMessageExists(Func<InMessage, bool> predicate);

        /// <summary>
        /// Select all the found 'EbmsMessageIds' in the given datastore.
        /// </summary>
        /// <param name="searchedMessageIds">Collection of 'EbmsMessageIds' to be search for.</param>
        /// <returns></returns>
        IEnumerable<string> SelectExistingInMessageIds(IEnumerable<string> searchedMessageIds);

        /// <summary>
        /// Search all the found 'RefToMessageIds' in the given datastore.
        /// </summary>
        /// <param name="searchedMessageIds"></param>
        /// <returns></returns>
        IEnumerable<string> SelectExistingRefInMessageIds(IEnumerable<string> searchedMessageIds);

        /// <summary>
        /// Gets the reception awareness.
        /// </summary>
        /// <param name="messageIds">The message ids.</param>
        /// <returns></returns>
        IEnumerable<ReceptionAwareness> GetReceptionAwareness(IEnumerable<string> messageIds);

        /// <summary>
        /// Selects the in messages.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="messageId"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        TResult GetInMessageData<TResult>(string messageId, Func<InMessage, TResult> selection);

        /// <summary>
        /// Firsts the or default out message.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        TResult GetOutMessageData<TResult>(string messageId, Func<OutMessage, TResult> selection);

        /// <summary>
        /// Inserts the in message.
        /// </summary>
        /// <param name="inMessage">The in message.</param>
        void InsertInMessage(InMessage inMessage);

        /// <summary>
        /// Inserts the out message.
        /// </summary>
        /// <param name="outMessage">The out message.</param>
        void InsertOutMessage(OutMessage outMessage);

        /// <summary>
        /// Inserts the in exception.
        /// </summary>
        /// <param name="inException">The in exception.</param>
        void InsertInException(InException inException);

        /// <summary>
        /// Inserts the out exception.
        /// </summary>
        /// <param name="outException">The out exception.</param>
        void InsertOutException(OutException outException);

        /// <summary>
        /// Inserts the reception awareness.
        /// </summary>
        /// <param name="receptionAwareness">The reception awareness.</param>
        void InsertReceptionAwareness(ReceptionAwareness receptionAwareness);

        /// <summary>
        /// Updates the in message.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateInMessage(string messageId, Action<InMessage> updateAction);

        /// <summary>
        /// Updates the out message.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateOutMessage(string messageId, Action<OutMessage> updateAction);

        /// <summary>
        /// Updates the in exception.
        /// </summary>
        /// <param name="refToMessageId">The reference to message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateInException(string refToMessageId, Action<InException> updateAction);

        /// <summary>
        /// Updates the out exception.
        /// </summary>
        /// <param name="refToMessageId">The reference to message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateOutException(string refToMessageId, Action<OutException> updateAction);

        /// <summary>
        /// Updates the reception awareness.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateReceptionAwareness(string messageId, Action<ReceptionAwareness> updateAction);
    }
}