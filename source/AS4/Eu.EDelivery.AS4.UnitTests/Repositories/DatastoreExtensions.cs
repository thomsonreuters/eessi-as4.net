using System;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    internal static class DatastoreExtensions
    {
        /// <summary>
        /// Gets the <see cref="InMessage"/> instance for a given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="createContext">The factory containing the datastore to get the record from</param>
        /// <param name="predicate">The predicate to locate the <see cref="InMessage"/> record</param>
        /// <returns></returns>
        public static InMessage GetInMessage(this Func<DatastoreContext> createContext, Func<InMessage, bool> predicate)
        {
            return RetrieveEntity(createContext, ctx => ctx.InMessages.Where(predicate).FirstOrDefault());
        }

        /// <summary>
        /// Gets the <see cref="RetryReliability"/> instance for a given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="createContext">The factory containing the datastore to get the record from</param>
        /// <param name="predicate">The predicate to locate the <see cref="RetryReliability"/> record</param>
        /// <returns></returns>
        public static RetryReliability GetRetryReliability(
            this Func<DatastoreContext> createContext,
            Func<RetryReliability, bool> predicate)
        {
            return RetrieveEntity(createContext, ctx => ctx.RetryReliability.Where(predicate).FirstOrDefault());
        }

        /// <summary>
        /// Inserts the out message.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="message">The message.</param>
        /// <returns>The OutMessage that has been inserted</returns>
        public static OutMessage InsertOutMessage(this Func<DatastoreContext> createContext, OutMessage message, bool withReceptionAwareness)
        {
            using (DatastoreContext context = createContext())
            {
                context.OutMessages.Add(message);
                context.SaveChanges();

                if (withReceptionAwareness)
                {
                    context.Add(new ReceptionAwareness(message.Id, message.EbmsMessageId));
                    context.SaveChanges();
                }

                return message;
            }
        }

        /// <summary>
        /// Inserts the in message.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="message">The message.</param>
        public static InMessage InsertInMessage(this Func<DatastoreContext> createContext, InMessage message)
        {
            using (DatastoreContext context = createContext())
            {
                context.InMessages.Add(message);
                context.SaveChanges();

                return message;
            }
        }

        /// <summary>
        /// Inserts the in exception.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="inException">The in exception.</param>
        public static InException InsertInException(this Func<DatastoreContext> createContext, InException inException)
        {
            using (DatastoreContext context = createContext())
            {
                context.InExceptions.Add(inException);
                context.SaveChanges();

                return inException;
            }
        }

        /// <summary>
        /// Inserts the out exception.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="outException">The out exception.</param>
        public static OutException InsertOutException(this Func<DatastoreContext> createContext, OutException outException)
        {
            using (DatastoreContext context = createContext())
            {
                context.OutExceptions.Add(outException);
                context.SaveChanges();

                return outException;
            }
        }

        /// <summary>
        /// Inserts the reception awareness.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="ra">The ra.</param>
        public static void InsertReceptionAwareness(this Func<DatastoreContext> createContext, ReceptionAwareness ra)
        {
            using (DatastoreContext context = createContext())
            {
                context.ReceptionAwareness.Add(ra);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Inserts the retry reliability on a given datastore.
        /// </summary>
        /// <param name="createContext">The factory containing the datastore to insert the <see cref="RetryReliability"/> instance</param>
        /// <param name="r">The <see cref="RetryReliability"/> instance to insert</param>
        public static void InsertRetryReliability(this Func<DatastoreContext> createContext, RetryReliability r)
        {
            using (DatastoreContext context = createContext())
            {
                context.RetryReliability.Add(r);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Asserts the in message.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertInMessage(this Func<DatastoreContext> createContext, string id, Action<InMessage> assertion)
        {
            assertion(
                RetrieveEntity(
                    createContext, 
                    c => c.InMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(id))));
        }

        /// <summary>
        /// Asserts the in message with reference to message identifier.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="refToMessageId">The reference to message identifier.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertInMessageWithRefToMessageId(
            this Func<DatastoreContext> createContext, 
            string refToMessageId, 
            Action<InMessage> assertion)
        {
            assertion(
                RetrieveEntity(
                    createContext, 
                    c => c.InMessages.FirstOrDefault(m => m.EbmsRefToMessageId.Equals(refToMessageId))));
        }

        /// <summary>
        /// Asserts the out message.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertOutMessage(
            this Func<DatastoreContext> createContext,
            string id,
            Action<OutMessage> assertion)
        {
            assertion(
                RetrieveEntity(
                    createContext, 
                    c => c.OutMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(id))));
        }

        /// <summary>
        /// Asserts the in exception.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertInException(this Func<DatastoreContext> createContext, Action<InException> assertion)
        {
            assertion(RetrieveEntity(createContext, c => c.InExceptions.FirstOrDefault()));
        }

        /// <summary>
        /// Asserts the in exception.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertInException(
            this Func<DatastoreContext> createContext,
            string id,
            Action<InException> assertion)
        {
            assertion(
                RetrieveEntity(
                    createContext, 
                    c => c.InExceptions.FirstOrDefault(e => e.EbmsRefToMessageId.Equals(id))));
        }

        /// <summary>
        /// Asserts the out exception.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertOutException(this Func<DatastoreContext> createContext, Action<OutException> assertion)
        {
            assertion(
                RetrieveEntity(
                    createContext, 
                    c => c.OutExceptions.FirstOrDefault()));
        }

        /// <summary>
        /// Asserts the out exception.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertOutException(this Func<DatastoreContext> createContext, string messageId, Action<OutException> assertion)
        {
            assertion(
                RetrieveEntity(
                    createContext,
                    c => c.OutExceptions.FirstOrDefault(e => e.EbmsRefToMessageId.Equals(messageId))));
        }

        /// <summary>
        /// Asserts the related <see cref="RetryReliability"/> entry for a given <see cref="InMessage"/> identifier.
        /// </summary>
        /// <param name="createContext">The factory containing the datastore where to assert on</param>
        /// <param name="messageId">The message identifier to locate the related <see cref="InMessage"/></param>
        /// <param name="assertion">The assertion to run on the found <see cref="RetryReliability"/></param>
        public static void AssertRetryRelatedInMessage(
            this Func<DatastoreContext> createContext,
            long messageId,
            Action<RetryReliability> assertion)
        {
            AssertRetryRelated(createContext, messageId, rr => rr.RefToInMessageId.Value, assertion);
        }

        /// <summary>
        /// Asserts the related <see cref="RetryReliability"/> entry for a given <see cref="OutMessage"/> identifier.
        /// </summary>
        /// <param name="createContext">The factory containing the datastore where to assert on</param>
        /// <param name="messageId">The message identifier to locate the related <see cref="OutMessage"/></param>
        /// <param name="assertion">The assertion to run on the found <see cref="RetryReliability"/></param>
        public static void AssertRetryRelatedOutMessage(
            this Func<DatastoreContext> createContext,
            long messageId,
            Action<RetryReliability> assertion)
        {
            AssertRetryRelated(createContext, messageId, rr => rr.RefToOutMessageId.Value, assertion);
        }

        /// <summary>
        /// Asserts the related <see cref="RetryReliability"/> entry for a given <see cref="InException"/> identifier.
        /// </summary>
        /// <param name="createContext">The factory containing the datastore where to assert on</param>
        /// <param name="exceptionId">The exception identifier to locate the related <see cref="InException"/></param>
        /// <param name="assertion">The assertion to run on the found <see cref="RetryReliability"/></param>
        public static void AssertRetryRelatedInException(
            this Func<DatastoreContext> createContext,
            long exceptionId,
            Action<RetryReliability> assertion)
        {
            AssertRetryRelated(createContext, exceptionId, rr => rr.RefToInExceptionId.Value, assertion);
        }

        /// <summary>
        /// Asserts the related <see cref="RetryReliability"/> entry for a given <see cref="OutException"/> identifier.
        /// </summary>
        /// <param name="createContext">The factory containing the datastore where to assert on</param>
        /// <param name="exceptionId">The exception identifier to locate the related <see cref="OutException"/></param>
        /// <param name="assertion">The assertion to run on the found <see cref="RetryReliability"/></param>
        public static void AssertRetryRelatedOutException(
            this Func<DatastoreContext> createContext,
            long exceptionId,
            Action<RetryReliability> assertion)
        {
            AssertRetryRelated(createContext, exceptionId, rr => rr.RefToOutExceptionId.Value, assertion);
        }

        private static void AssertRetryRelated(
            Func<DatastoreContext> createContext,
            long messageId,
            Func<RetryReliability, long> getter,
            Action<RetryReliability> assertion)
        {
            assertion(
                RetrieveEntity(
                    createContext,
                    ctx => ctx.RetryReliability.FirstOrDefault(rr => getter(rr) == messageId)));
        }

        private static T RetrieveEntity<T>(Func<DatastoreContext> createContext, Func<DatastoreContext, T> selection)
        {
            using (DatastoreContext context = createContext())
            {
                return selection(context);
            }
        }
    }
}
