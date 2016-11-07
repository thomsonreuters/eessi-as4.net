using System;
using Eu.EDelivery.AS4.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Data Store Connection Test Setup
    /// </summary>
    public class GivenDatastoreFacts
    {
        protected readonly DbContextOptions<DatastoreContext> Options;

        /// <summary>
        /// Create a Default Datastore Facts
        /// </summary>
        public GivenDatastoreFacts()
        {
            this.Options = CreateNewContextOptions();
        }

        protected DbContextOptions<DatastoreContext> CreateNewContextOptions()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            IServiceProvider serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var builder = new DbContextOptionsBuilder<DatastoreContext>();
            builder.UseInMemoryDatabase()
                .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }
    }
}