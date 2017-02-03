using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.ServiceHandler.Agents;
using Eu.EDelivery.AS4.ServiceHandler.Builder;

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

            var cancellationTokenSource = new CancellationTokenSource();
            Task task = kernel.StartAsync(cancellationTokenSource.Token);
            task.ContinueWith(
                x =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(x.Exception?.ToString());
                },
                TaskContinuationOptions.OnlyOnFaulted);

            Console.ReadLine();

            Console.WriteLine(@"Stopping...");
            cancellationTokenSource.Cancel();

            task.GetAwaiter().GetResult();
            Console.WriteLine($@"Stopped: {task.Status}");

            if (task.IsFaulted && task.Exception != null)
                Console.WriteLine(task.Exception.ToString());

            Config.Instance.Dispose();

            Console.ReadLine();
        }

        private static Kernel CreateKernel()
        {
            Config config = Config.Instance;
            Registry registry = Registry.Instance;

            config.Initialize();

            StartFeInProcess();

            if (!config.IsInitialized) return null;

            string certificateTypeRepository = config.GetSetting("CertificateRepository");
            registry.CertificateRepository = new GenericTypeBuilder().SetType(certificateTypeRepository).Build<ICertificateRepository>();
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
    }
}