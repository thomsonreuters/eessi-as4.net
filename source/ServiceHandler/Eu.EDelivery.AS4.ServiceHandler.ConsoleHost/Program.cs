using System;
using System.Diagnostics;
using System.Reflection;
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
                        NLog.LogManager.GetCurrentClassLogger().Fatal(x.Exception?.ToString());
                    },
                    TaskContinuationOptions.OnlyOnFaulted);

                Task frontEndTask = StartFeInProcess(cancellationTokenSource.Token);
                Task payloadServiceTask = StartPayloadServiceInProcess(cancellationTokenSource.Token);

                ConsoleKeyInfo key;

                do
                {
                    Console.WriteLine("Press following charaters during the running of the component to:");
                    Console.WriteLine("    c    Clears the screen");
                    Console.WriteLine("    q    Quites the application");
                    Console.WriteLine("    r    Restarts the application");

                    key = Console.ReadKey();

                    switch (key.Key)
                    {
                        case ConsoleKey.C:
                            Console.Clear();
                            break;
                        case ConsoleKey.R:
                            string fileName = Assembly.GetExecutingAssembly().Location;
                            Process.Start(fileName);

                            Environment.Exit(0);
                            break;                            
                    }


                } while (key.Key != ConsoleKey.Q);

                Console.WriteLine(@"Stopping...");
                cancellationTokenSource.Cancel();

                StopTask(task);
                StopTask(frontEndTask);
                StopTask(payloadServiceTask);

                Console.WriteLine($@"Stopped: {task.Status}");

                if (task.IsFaulted && task.Exception != null)
                {
                    Console.WriteLine(task.Exception.ToString());
                    Console.WriteLine("Press enter to terminate ...");
                    Console.ReadLine();
                }
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

            try
            {
                config.Initialize("settings.xml");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

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

        private static Task StartFeInProcess(CancellationToken cancellationToken)
        {
            if (!Config.Instance.FeInProcess)
            {
                return Task.CompletedTask;
            }

            var frontEndTask = Task.Factory.StartNew(() => Fe.Program.StartInProcess(cancellationToken), cancellationToken);

            frontEndTask.ContinueWith(t => LogExceptions(t), TaskContinuationOptions.OnlyOnFaulted);

            return frontEndTask;            
        }

        private static Task StartPayloadServiceInProcess(CancellationToken cancellationToken)
        {
            if (!Config.Instance.PayloadServiceInProcess)
            {
                return Task.CompletedTask;
            }

            var payloadServiceTask = Task.Factory.StartNew(() => PayloadService.Program.Start(cancellationToken), cancellationToken);

            payloadServiceTask.ContinueWith(t => LogExceptions(t), TaskContinuationOptions.OnlyOnFaulted);

            return payloadServiceTask;
        }

        private static void LogExceptions(Task task)
        {
            if (task.Exception?.InnerExceptions != null)
            {
                foreach (Exception ex in task.Exception?.InnerExceptions)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.Message);
                }
            }
        }

        private static void StopTask(Task task)
        {
            try
            {
                task.GetAwaiter().GetResult();
            }
            catch (AggregateException exception)
            {
                exception.Handle(e => e is TaskCanceledException);
            }
            finally
            {
                task.Dispose();
            }
        }
    }
}