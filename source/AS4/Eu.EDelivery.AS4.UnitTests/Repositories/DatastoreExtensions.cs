using System;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    internal static class DatastoreExtensions
    {
        /// <summary>
        /// Inserts the out message.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="message">The message.</param>
        public static void InsertOutMessage(this Func<DatastoreContext> createContext, OutMessage message)
        {
            using (DatastoreContext context = createContext())
            {
                context.OutMessages.Add(message);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Inserts the in message.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="message">The message.</param>
        public static void InsertInMessage(this Func<DatastoreContext> createContext, InMessage message)
        {
            using (DatastoreContext context = createContext())
            {
                context.InMessages.Add(message);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Inserts the reception awareness into the Datastore.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="awareness">The awareness.</param>
        public static void InsertReceptionAwareness(
            this Func<DatastoreContext> createContext,
            ReceptionAwareness awareness)
        {
            using (DatastoreContext context = createContext())
            {
                context.ReceptionAwareness.Add(awareness);
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
            Func<DatastoreContext, InMessage> selection =
                c => c.InMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(id));

            assertion(RetrieveEntity(createContext, selection));
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
            Func<DatastoreContext, OutMessage> selection =
                c => c.OutMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(id));

            assertion(RetrieveEntity(createContext, selection));
        }

        /// <summary>
        /// Asserts the in exception.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertInException(this Func<DatastoreContext> createContext, Action<InException> assertion)
        {
            Func<DatastoreContext, InException> selection =
                c => c.InExceptions.FirstOrDefault();

            assertion(RetrieveEntity(createContext, selection));
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
            Func<DatastoreContext, InException> selection =
                c => c.InExceptions.FirstOrDefault(e => e.EbmsRefToMessageId.Equals(id));

            assertion(RetrieveEntity(createContext, selection));
        }

        /// <summary>
        /// Asserts the out exception.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertOutException(this Func<DatastoreContext> createContext, Action<OutException> assertion)
        {
            Func<DatastoreContext, OutException> selection =
                c => c.OutExceptions.FirstOrDefault();

            assertion(RetrieveEntity(createContext, selection));
        }

        /// <summary>
        /// Asserts the out exception.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="assertion">The assertion.</param>
        public static void AssertOutException(this Func<DatastoreContext> createContext, string messageId, Action<OutException> assertion)
        {
            Func<DatastoreContext, OutException> selection =
                c => c.OutExceptions.FirstOrDefault(e => e.EbmsRefToMessageId.Equals(messageId));

            assertion(RetrieveEntity(createContext, selection));
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
