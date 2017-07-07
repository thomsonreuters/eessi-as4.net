using System;
using System.Diagnostics;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

namespace Eu.EDelivery.AS4.IntegrationTests.Fixture
{
    /// <summary>
    /// Holodeck Fixture which handles the Holodeck instances.
    /// </summary>
    public class HolodeckFixture : IDisposable
    {
        private readonly ParentProcess _parentProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="HolodeckFixture"/> class.
        /// </summary>
        public HolodeckFixture()
        {
            var service = new FileSystemService();
            service.CleanUpFiles(holodeck_A_input_path);
            service.CleanUpFiles(holodeck_B_input_path);

            service.CleanUpFiles(holodeck_A_pmodes);
            service.CleanUpFiles(holodeck_B_pmodes);

            service.CleanUpFiles(holodeck_A_output_path);
            service.CleanUpFiles(holodeck_B_output_path);

            service.RemoveDirectory(holodeck_A_db_path);
            service.RemoveDirectory(holodeck_B_db_path);
            
            Process holodeckA = Process.Start(@"C:\Program Files\Java\holodeck\holodeck-b2b-A\bin\startServer.bat");
            Process holodeckB = Process.Start(@"C:\Program Files\Java\holodeck\holodeck-b2b-B\bin\startServer.bat");

            _parentProcess = new ParentProcess(holodeckA, holodeckB);

            // Make sure the Holodeck MSH's are started before continuing.
            System.Threading.Thread.Sleep(6000);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _parentProcess.KillMeAndChildren();
            _parentProcess.Dispose();
        }
    }

    /// <summary>
    /// This class has no code, and is never created. Its purpose is simply
    /// to be the place to apply <see cref="CollectionDefinitionAttribute" /> and all the
    /// <see cref="ICollectionFixture{TFixture}" /> interfaces.
    /// </summary>
    [CollectionDefinition(CollectionId)]
    public class HolodeckCollection : ICollectionFixture<HolodeckFixture>
    {
        public const string CollectionId = "Holodeck collection";
    }
}