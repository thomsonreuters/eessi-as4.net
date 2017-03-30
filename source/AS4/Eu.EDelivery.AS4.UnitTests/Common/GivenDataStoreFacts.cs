using System;
using Eu.EDelivery.AS4.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Data Store Connection Test Setup
    /// </summary>
    [Collection("Tests that impact datastore")]  // Tests that belong to the same collection do not run in parallel.
    public class GivenDatastoreFacts
    {
        private IServiceProvider _serviceProvider;

        protected DbContextOptions<DatastoreContext> Options { get; }

        protected Func<DatastoreContext> GetDataStoreContext { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenDatastoreFacts"/> class. 
        /// </summary>
        public GivenDatastoreFacts()
        {
            Options = CreateNewContextOptions();
            GetDataStoreContext = () => new DatastoreContext(Options);
            Registry.Instance.CreateDatastoreContext = () => new DatastoreContext(Options);
        }

        private DbContextOptions<DatastoreContext> CreateNewContextOptions()
        {
            if (_serviceProvider == null)
            {
                ResetInMemoryDatabase();
            }

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            return new DbContextOptionsBuilder<DatastoreContext>()
                .UseInMemoryDatabase()
                .UseInternalServiceProvider(_serviceProvider)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }

        private void ResetInMemoryDatabase()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
        }
    }
}