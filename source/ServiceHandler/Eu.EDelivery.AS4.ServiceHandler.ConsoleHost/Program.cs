using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.ServiceHandler.Agents;

namespace Eu.EDelivery.AS4.ServiceHandler.ConsoleHost
{
    public class Program
    {
        public static void Main()
        {
            Console.SetWindowSize(Console.LargestWindowWidth, Console.WindowHeight);

            Kernel kernel = CreateKernel();
            if (kernel == null)
            {
                Console.ReadLine();
                return;
            }
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                Task task = kernel.StartAsync(cancellationTokenSource.Token);
                task.ContinueWith(
                    x =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        //NLog.LogManager.GetCurrentClassLogger().Fatal(x.Exception?.ToString());
                    },
                    TaskContinuationOptions.OnlyOnFaulted);

                ConsoleKeyInfo key;

                do
                {
                    Console.WriteLine("Press c to clear the screen, q to stop.");

                    key = Console.ReadKey();

                    switch (key.Key)
                    {
                        case ConsoleKey.C:
                            Console.Clear();
                            break;
                    }


                } while (key.Key != ConsoleKey.Q);


                Console.WriteLine(@"Stopping...");
                cancellationTokenSource.Cancel();

                task.GetAwaiter().GetResult();
                Console.WriteLine($@"Stopped: {task.Status}");

                if (task.IsFaulted && task.Exception != null)
                    Console.WriteLine(task.Exception.ToString());
            }
            finally
            {
                kernel.Dispose();
                Config.Instance.Dispose();
            }

            Console.ReadLine();
        }

        private static Kernel CreateKernel()
        {
            Config config = Config.Instance;
            Registry registry = Registry.Instance;

            config.Initialize();

            StartFeInProcess();
            StartPayloadServiceInProcess();

            if (!config.IsInitialized)
            {
                return null;
            }

            string certificateTypeRepository = config.GetSetting("CertificateRepository");
            if (!String.IsNullOrWhiteSpace(certificateTypeRepository))
            {
                registry.CertificateRepository = GenericTypeBuilder.FromType(certificateTypeRepository).Build<ICertificateRepository>();
            }
            else
            {
                registry.CertificateRepository = new CertificateRepository();   
            }

            registry.CreateDatastoreContext = () => new DatastoreContext(config);

            var agentProvider = new AgentProvider(config);
            return new Kernel(agentProvider.GetAgents());
        }

        private static void StartFeInProcess()
        {
            if (!Config.Instance.FeInProcess) return;

            Task.Factory.StartNew(() =>
            {
                Fe.Program.Main(new[] { "inprocess" });
            });
        }

        private static void StartPayloadServiceInProcess()
        {
            if (!Config.Instance.PayloadServiceInProcess) return;

            Task.Factory.StartNew(
                () =>
                {
                    PayloadService.Program.Main(new string[0]);
                });
        }
    }
}