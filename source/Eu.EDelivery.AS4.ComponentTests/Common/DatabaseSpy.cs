using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

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
        public InMessage GetInMessageFor(Func<InMessage, bool> expression)
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
        public OutMessage GetOutMessageFor(Func<OutMessage, bool> expression)
        {
            using (var context = new DatastoreContext(_configuration))
            {
                return context.OutMessages.Where(expression).FirstOrDefault();
            }
        }

        public void InsertOutMessage(OutMessage message)
        {
            using (var context = new DatastoreContext(_configuration))
            {
                context.OutMessages.Add(message);

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
    }
}