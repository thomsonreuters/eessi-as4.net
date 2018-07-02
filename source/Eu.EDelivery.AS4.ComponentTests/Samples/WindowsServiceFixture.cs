using System;
using System.ServiceProcess;
using Eu.EDelivery.AS4.TestUtils;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Samples
{
    public class WindowsServiceFixture : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsServiceFixture"/> class.
        /// </summary>
        public WindowsServiceFixture()
        {
            Computer.RunCommand(@"& .\install-windows-service.bat");
            StartWindowsService();
        }

        private static void StartWindowsService()
        {
            using (var controller = new ServiceController("AS4Service"))
            {
                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Computer.RunCommand(@"& .\uninstall-windows-service.bat");
        }
    }

    /// <summary>
    /// This class has no code, and is never created. Its purpose is simply
    /// to be the place to apply <see cref="CollectionDefinitionAttribute" /> and all the
    /// <see cref="ICollectionFixture{TFixture}" /> interfaces.
    /// </summary>
    [CollectionDefinition(CollectionId)]
    public class WindowsServiceCollection : ICollectionFixture<WindowsServiceFixture>
    {
        public const string CollectionId = "Windows Service collection";
    }
}
