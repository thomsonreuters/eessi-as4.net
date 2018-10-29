using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    internal class DatastoreSpy
    {
        private readonly IConfig _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreSpy" /> class.
        /// </summary>
        /// <param name="configuration">The local configuration implementation.</param>
        public DatastoreSpy(IConfig configuration)
        {
            _configuration = configuration;
        }

        public void InsertSmpConfiguration(SmpConfiguration config)
        {
            using (var context = new DatastoreContext(_configuration))
            {
                context.SmpConfigurations.Add(config);
                context.SaveChanges();
            }
        }
    }
}
