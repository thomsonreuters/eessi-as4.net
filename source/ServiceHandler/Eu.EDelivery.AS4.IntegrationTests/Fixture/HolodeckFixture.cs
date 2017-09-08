using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
            if (Holodeck.HolodeckALocations == null)
            {
                throw new ConfigurationErrorsException("Holodeck A could not be found on this machine");
            }

            if (Holodeck.HolodeckBLocations == null)
            {
                throw new ConfigurationErrorsException("Holodeck B could not be found on this machine");
            }

            var service = new FileSystemService();
            service.CleanUpFiles(Holodeck.HolodeckALocations.InputPath); 
            service.CleanUpFiles(Holodeck.HolodeckBLocations.InputPath);

            service.CleanUpFiles(Holodeck.HolodeckALocations.PModePath);
            service.CleanUpFiles(Holodeck.HolodeckBLocations.PModePath); 

            service.CleanUpFiles(Holodeck.HolodeckALocations.OutputPath); 
            service.CleanUpFiles(Holodeck.HolodeckBLocations.OutputPath);  

            service.RemoveDirectory(Holodeck.HolodeckALocations.DbPath); 
            service.RemoveDirectory(Holodeck.HolodeckBLocations.DbPath);   

            Process holodeckA = StartHolodeck(Holodeck.HolodeckALocations.BinaryPath); 
            Process holodeckB = StartHolodeck(Holodeck.HolodeckBLocations.BinaryPath); 

            _parentProcess = new ParentProcess(holodeckA, holodeckB);

            // Make sure the Holodeck MSH's are started before continuing.
            System.Threading.Thread.Sleep(6000);
        }

        private static Process StartHolodeck(string executablePath)
        {
            Console.WriteLine($@"Try starting Holodeck at {executablePath}");

            Process p = new Process();

            p.StartInfo.FileName = executablePath;
            p.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(executablePath);
            p.StartInfo.CreateNoWindow = false;


            try
            {
                if (p.Start() == false)
                {
                    throw new InvalidOperationException($"Unable to start holodeck. Exitcode = {p.ExitCode}");
                }

                Console.WriteLine($@"Holodeck {p.ProcessName} started.  Process Id: {p.Id}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException);
                }
                throw;
            }

            return p;
        }

        private bool _isDisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
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