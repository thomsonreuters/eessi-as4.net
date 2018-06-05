using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    public class DatabaseSpy
    {
        private readonly IConfig _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSpy"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public DatabaseSpy(IConfig configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the first <see cref="InMessage"/> instance that matches the given criteria in the <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression to search for a single <see cref="InMessage"/>.</param>
        /// <returns></returns>
        public InMessage GetInMessageFor(Expression<Func<InMessage, bool>> expression)
        {
            using (var context = new DatastoreContext(_configuration))
            {
                return context.InMessages.Where(expression).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the first <see cref="OutMessage"/> instance that matches the given criterium in the <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression to search for a single <see cref="OutMessage"/>.</param>
        /// <returns></returns>
        public OutMessage GetOutMessageFor(Expression<Func<OutMessage, bool>> expression)
        {
            using (var context = new DatastoreContext(_configuration))
            {
                return context.OutMessages.Where(expression).FirstOrDefault();
            }
        }

        /// <summary>
        /// Inserts the given <see cref="OutMessage"/> into the <see cref="DatastoreContext"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        public void InsertOutMessage(OutMessage message) => ChangeContext(ctx => ctx.OutMessages.Add(message));

        /// <summary>
        /// Inserts the given <see cref="InMessage"/> into the <see cref="DatastoreContext"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        public void InsertInMessage(InMessage message) => ChangeContext(ctx => ctx.InMessages.Add(message));

        /// <summary>
        /// Inserts the given <see cref="OutException"/> into the <see cref="DatastoreContext"/>.
        /// </summary>
        /// <param name="ex">The message.</param>
        public void InsertOutException(OutException ex) => ChangeContext(ctx => ctx.OutExceptions.Add(ex));

        /// <summary>
        /// Inserts the given <see cref="InException"/> into the <see cref="DatastoreContext"/>.
        /// </summary>
        /// <param name="ex">The message.</param>
        public void InsertInException(InException ex) => ChangeContext(ctx => ctx.InExceptions.Add(ex));

        private void ChangeContext(Action<DatastoreContext> changement)
        {
            using (var context = new DatastoreContext(_configuration))
            {
                changement(context);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the <see cref="InException"/> instances for a given <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression to search for <see cref="InException"/> instances.</param>
        /// <returns></returns>
        public IEnumerable<InException> GetInExceptions(Expression<Func<InException, bool>> expression)
        {
            using (var context = new DatastoreContext(_configuration))
            {
                return context.InExceptions.Where(expression).ToList();
            }
        }

        /// <summary>
        /// Gets the <see cref="OutMessage"/> entities where the <see cref="OutMessage.EbmsMessageId"/> is one of the given <paramref name="ebmsMessageIds"/>.
        /// </summary>
        /// <param name="ebmsMessageIds">The ebms message ids.</param>
        /// <returns></returns>
        public IEnumerable<OutMessage> GetOutMessages(params string[] ebmsMessageIds)
        {
            Console.WriteLine(@"Get OutMessage(s) where EbmsMessageId = " + String.Join(", ", ebmsMessageIds));
            return UseContext(
                ctx => ctx.OutMessages.Where(m => ebmsMessageIds.Contains(m.EbmsMessageId)).ToArray());
        }

        /// <summary>
        /// Gets the <see cref="InMessage"/> entities where the <see cref="InMessage.EbmsMessageId"/> is one of the given <paramref name="ebmsMessageIds"/>.
        /// </summary>
        /// <param name="ebmsMessageIds">The ebms message ids.</param>
        /// <returns></returns>
        public IEnumerable<InMessage> GetInMessages(params string[] ebmsMessageIds)
        {
            Console.WriteLine(@"Get InMessage(s) where EbmsMessageId = " + String.Join(", ", ebmsMessageIds));
            return UseContext(
                ctx => ctx.InMessages.Where(m => ebmsMessageIds.Contains(m.EbmsMessageId)).ToArray());
        }

        /// <summary>
        /// Gets the <see cref="OutException"/> entities where the <see cref="OutException.EbmsRefToMessageId"/> is one of the given <paramref name="ebmsMessageIds"/>.
        /// </summary>
        /// <param name="ebmsMessageIds">The ebms message ids.</param>
        /// <returns></returns>
        public IEnumerable<OutException> GetOutExceptions(params string[] ebmsMessageIds)
        {
            Console.WriteLine(@"Get OutException(s) where EbmsMessageId = " + String.Join(", ", ebmsMessageIds));
            return UseContext(
                ctx => ctx.OutExceptions.Where(ex => ebmsMessageIds.Contains(ex.EbmsRefToMessageId)).ToArray());
        }

        /// <summary>
        /// Gets the <see cref="InException"/> entities where the <see cref="InException.EbmsRefToMessageId"/> is one of the given <paramref name="ebmsMessageIds"/>.
        /// </summary>
        /// <param name="ebmsMessageIds">The ebms message ids.</param>
        /// <returns></returns>
        public IEnumerable<InException> GetInExceptions(params string[] ebmsMessageIds)
        {
            Console.WriteLine(@"Get InException(s) where EbmsMessageId = " + String.Join(", ", ebmsMessageIds));
            return UseContext(
                ctx => ctx.InExceptions.Where(ex => ebmsMessageIds.Contains(ex.EbmsRefToMessageId)).ToArray());
        }

        private T UseContext<T>(Func<DatastoreContext, T> selector)
        {
            using (var context = new DatastoreContext(_configuration))
            {
                return selector(context);
            }
        }

        internal void ClearDatabase()
        {
            Console.WriteLine(@"Clear database tables");
            using (var context = new DatastoreContext(_configuration))
            {
                context.Database.ExecuteSqlCommand("DELETE FROM InExceptions");
                context.Database.ExecuteSqlCommand("DELETE FROM OutExceptions");
                context.Database.ExecuteSqlCommand("DELETE FROM InMessages");
                context.Database.ExecuteSqlCommand("DELETE FROM OutMessages");
            }
        }
    }
}