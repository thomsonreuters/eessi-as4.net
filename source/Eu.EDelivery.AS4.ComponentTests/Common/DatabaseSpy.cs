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
        public DatabaseSpy(IConfig configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<InException> GetInExceptions(Expression<Func<InException, bool>>  expression)
        {
            using (var dbContext = new DatastoreContext(_configuration))
            {
                return dbContext.InExceptions.Where(expression).ToList();
            }
        }


    }
}
