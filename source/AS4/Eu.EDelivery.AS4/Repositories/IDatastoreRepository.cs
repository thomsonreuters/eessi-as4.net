using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Repositories
{
    public interface IDatastoreRepository
    {
        #region InMessage functionality

        /// <summary>
        /// Inserts the in message.
        /// </summary>
        /// <param name="inMessage">The in message.</param>
        void InsertInMessage(InMessage inMessage);


        /// <summary>
        /// Verifies if there exists an InMessage that matches the given predicate.
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
        /// Selects the in messages.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="messageId"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        TResult GetInMessageData<TResult>(string messageId, Func<InMessage, TResult> selection);

        /// <summary>
        /// Selects some information of specified InMessages.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="messageIds"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        IEnumerable<TResult> GetInMessagesData<TResult>(IEnumerable<string> messageIds, Func<InMessage, TResult> selection);

        /// <summary>
        /// Updates the in message.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateInMessage(string messageId, Action<InMessage> updateAction);

        [Obsolete("Sqlite is not supported by this method")]
        void UpdateInMessages(Expression<Func<InMessage, bool>> predicate, Expression<Func<InMessage, InMessage>> updateAction);

        void UpdateInMessages(Expression<Func<InMessage, bool>> predicate, Action<InMessage> updateAction);

        #endregion

        #region OutMessage functionality

        /// <summary>
        /// Verifies if there exists an OutMessage that matches the given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        bool OutMessageExists(Func<OutMessage, bool> predicate);

        /// <summary>
        /// Firsts the or default out message.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        TResult GetOutMessageData<TResult>(string messageId, Func<OutMessage, TResult> selection);

        /// <summary>
        /// Gets the out message data.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="where">The where.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        TResult GetOutMessageData<TResult>(Func<OutMessage, bool> where, Func<OutMessage, TResult> selection);

        /// <summary>
        /// Inserts the out message.
        /// </summary>
        /// <param name="outMessage">The out message.</param>
        void InsertOutMessage(OutMessage outMessage);

        /// <summary>
        /// Updates the out message.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateOutMessage(string messageId, Action<OutMessage> updateAction);

        [Obsolete("Sqlite is not supported by this method")]
        void UpdateOutMessages(Expression<Func<OutMessage, bool>> predicate, Expression<Func<OutMessage, OutMessage>> updateAction);

        void UpdateOutMessages(Expression<Func<OutMessage, bool>> predicate, Action<OutMessage> updateAction);

        #endregion

        #region InException functionality

        /// <summary>
        /// Inserts the in exception.
        /// </summary>
        /// <param name="inException">The in exception.</param>
        void InsertInException(InException inException);

        /// <summary>
        /// Updates the in exception.
        /// </summary>
        /// <param name="refToMessageId">The reference to message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateInException(string refToMessageId, Action<InException> updateAction);

        #endregion

        #region OutException functionality

        /// <summary>
        /// Inserts the out exception.
        /// </summary>
        /// <param name="outException">The out exception.</param>
        void InsertOutException(OutException outException);

        /// <summary>
        /// Updates the out exception.
        /// </summary>
        /// <param name="refToMessageId">The reference to message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateOutException(string refToMessageId, Action<OutException> updateAction);

        #endregion

        #region ReceptionAwareness Functionality

        /// <summary>
        /// Inserts the reception awareness.
        /// </summary>
        /// <param name="receptionAwareness">The reception awareness.</param>
        void InsertReceptionAwareness(ReceptionAwareness receptionAwareness);

        /// <summary>
        /// Updates the reception awareness.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateReceptionAwareness(string messageId, Action<ReceptionAwareness> updateAction);

        /// <summary>
        /// Gets the reception awareness.
        /// </summary>
        /// <param name="messageIds">The message ids.</param>
        /// <returns></returns>
        IEnumerable<ReceptionAwareness> GetReceptionAwareness(IEnumerable<string> messageIds);

        #endregion
    }
}