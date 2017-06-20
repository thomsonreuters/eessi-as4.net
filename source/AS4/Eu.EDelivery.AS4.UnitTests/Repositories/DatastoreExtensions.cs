using System;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    internal static class DatastoreExtensions
    {
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
