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
        bool InMessageExists(Expression<Func<InMessage, bool>> predicate);

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
        TResult GetInMessageData<TResult>(string messageId, Expression<Func<InMessage, TResult>> selection);

        /// <summary>
        /// Retrieves information for specified InMessages.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="where">The where.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        IEnumerable<TResult> GetInMessageData<TResult>(Expression<Func<InMessage, bool>> where, Expression<Func<InMessage, TResult>> selection);

        /// <summary>
        /// Selects some information of specified InMessages.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="messageIds"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        IEnumerable<TResult> GetInMessagesData<TResult>(IEnumerable<string> messageIds, Expression<Func<InMessage, TResult>> selection);

        /// <summary>
        /// Updates the in message.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateInMessage(string messageId, Action<InMessage> updateAction);


        void UpdateInMessages(Expression<Func<InMessage, bool>> predicate, Action<InMessage> updateAction);

        #endregion

        #region OutMessage functionality

        /// <summary>
        /// Verifies if there exists an OutMessage that matches the given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        bool OutMessageExists(Expression<Func<OutMessage, bool>> predicate);

        /// <summary>
        /// Retrieves the data of the OutMessage that has the specified <paramref name="messageId"/>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="messageId"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        TResult GetOutMessageData<TResult>(long messageId, Expression<Func<OutMessage, TResult>> selection);

        /// <summary>
        /// Gets the out message data.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="where">The where.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        IEnumerable<TResult> GetOutMessageData<TResult>(Expression<Func<OutMessage, bool>> where, Expression<Func<OutMessage, TResult>> selection);

        /// <summary>
        /// Inserts the out message.
        /// </summary>
        /// <param name="outMessage">The out message.</param>
        void InsertOutMessage(OutMessage outMessage);

        /// <summary>
        /// Updates the out message.
        /// </summary>
        /// <param name="outMessageId">The ID that uniquely identifies the OutMessage record that must be updated..</param>
        /// <param name="updateAction">The update action.</param>
        void UpdateOutMessage(long outMessageId, Action<OutMessage> updateAction);

        void UpdateOutMessages(Expression<Func<OutMessage, bool>> predicate, Action<OutMessage> updateAction);

        #endregion

        #region InException functionality

        /// <summary>
        /// Retrieves information for a specified InException using a <paramref name="refToMessageId"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="refToMessageId"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        IEnumerable<TResult> GetInExceptionsData<TResult>(
            string refToMessageId,
            Expression<Func<InException, TResult>> selection);

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
        /// Retrieves information for a specified OutException using a <paramref name="refToMessageId"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="refToMessageId"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        IEnumerable<TResult> GetOutExceptionsData<TResult>(
            string refToMessageId,
            Expression<Func<OutException, TResult>> selection);

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

        #region RetryReliability related functionality

        /// <summary>
        /// Gets a sequence of <see cref="RetryReliability"/> records based on a given <paramref name="predicate"/>,
        /// using a <paramref name="selector"/> to manipulate to a <typeparamref name="TResult"/> type.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        IEnumerable<TResult> GetRetryReliability<TResult>(
            Expression<Func<RetryReliability, bool>> predicate,
            Expression<Func<RetryReliability, TResult>> selector);

        /// <summary>
        /// Inserts the retry reliability information referencing a <see cref="InMessage"/>.
        /// </summary>
        /// <param name="reliability">The <see cref="RetryReliability"/> entity to insert</param>
        /// 
        void InsertRetryReliability(RetryReliability reliability);

        /// <summary>
        /// Inserts the retry reliability informations referencing <see cref="InMessage"/>'s.
        /// </summary>
        /// <param name="reliabilities">The <see cref="RetryReliability"/> entities to insert</param>
        void InsertRetryReliabilities(IEnumerable<RetryReliability> reliabilities);

        #endregion
    }
}