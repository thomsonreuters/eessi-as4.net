using System;
using System.ServiceProcess;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    public class WindowsServiceFixture : IDisposable
    {
        /// <summary>
        /// Ensures that the Windows Service is started.
        /// </summary>
        public void EnsureServiceIsStarted()
        {
            using (var controller = new ServiceController("AS4Service"))
            {
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running);

                    Console.WriteLine(@"Start AS4.NET Windows Service as Receiver MSH");
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            using (var controller = new ServiceController("AS4Service"))
            {
                if (controller.Status != ServiceControllerStatus.Stopped)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped); 
                }
            }
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
