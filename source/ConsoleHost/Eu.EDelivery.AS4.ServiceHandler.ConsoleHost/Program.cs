using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.ServiceHandler.Agents;
using NLog;

namespace Eu.EDelivery.AS4.ServiceHandler.ConsoleHost
{
    public class Program
    {
        public static void Main()
        {
            //Console.SetWindowSize(Console.LargestWindowWidth, Console.WindowHeight);

            Kernel kernel = CreateKernel();

            if (kernel == null)
            {
                Console.ReadLine();
                return;
            }

            try
            {
                var lifecycle = new AS4ComponentLifecycle(kernel);
                lifecycle.Start();

                ConsoleKeyInfo key;

                do
                {
                    Console.WriteLine(@"The following commands are available while the AS4.NET MSH is running:");
                    Console.WriteLine("\tc\t: Clears the screen");
                    Console.WriteLine("\tq\t: Quits the application");
                    Console.WriteLine("\tr\t: Restarts the application");

                    key = Console.ReadKey();

                    switch (key.Key)
                    {
                        case ConsoleKey.C:
                            Console.Clear();
                            break;
                        case ConsoleKey.R:
                            Console.WriteLine("Restarting...");
                            lifecycle.Stop();

                            kernel.Dispose();
                            Config.Instance.Dispose();

                            kernel = CreateKernel();
                            lifecycle = new AS4ComponentLifecycle(kernel);
                            lifecycle.Start();

                            break;
                    }
                }
                while (key.Key != ConsoleKey.Q);

                Console.WriteLine(@"Stopping...");
                Task task = lifecycle.Stop();

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
                kernel?.Dispose();
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

        private sealed class AS4ComponentLifecycle
        {
            private readonly CancellationTokenSource _cancellation;

            private readonly Kernel _kernel;
            private Task _kernelTask, _frontendTask, _payloadServiceTask;


            /// <summary>
            /// Initializes a new instance of the <see cref="AS4ComponentLifecycle" /> class.
            /// </summary>
            /// <param name="kernel">The kernel.</param>
            public AS4ComponentLifecycle(Kernel kernel)
            {
                _kernel = kernel;
                _cancellation = new CancellationTokenSource();

            }

            /// <summary>
            /// Starts this instance.
            /// </summary>
            public void Start()
            {
                _kernelTask = StartKernel();
                _frontendTask = StartFeInProcess(_cancellation.Token);
                _payloadServiceTask = StartPayloadServiceInProcess(_cancellation.Token);
            }

            private Task StartKernel()
            {
                Task task = _kernel.StartAsync(_cancellation.Token);

                task.ContinueWith(
                    x =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        LogManager.GetCurrentClassLogger().Fatal(x.Exception?.ToString());
                    },
                    TaskContinuationOptions.OnlyOnFaulted);

                return task;
            }

            private static Task StartFeInProcess(CancellationToken cancellationToken)
            {
                if (!Config.Instance.FeInProcess)
                {
                    return Task.CompletedTask;
                }

                Task task = Task.Factory
                    .StartNew(() => Fe.Program.StartInProcess(cancellationToken), cancellationToken);
                task.ContinueWith(LogExceptions, TaskContinuationOptions.OnlyOnFaulted);

                return task;
            }

            private static Task StartPayloadServiceInProcess(CancellationToken cancellationToken)
            {
                if (!Config.Instance.PayloadServiceInProcess)
                {
                    return Task.CompletedTask;
                }

                Task task = Task.Factory.StartNew(() => PayloadService.Program.Start(cancellationToken), cancellationToken);
                task.ContinueWith(LogExceptions, TaskContinuationOptions.OnlyOnFaulted);

                return task;
            }

            private static void LogExceptions(Task task)
            {
                if (task.Exception?.InnerExceptions == null)
                {
                    return;
                }

                foreach (Exception ex in task.Exception?.InnerExceptions)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.Message);
                }
            }

            /// <summary>
            /// Stops this instance.
            /// </summary>
            /// <returns></returns>
            public Task Stop()
            {
                _cancellation.Cancel();

                StopTask(_kernelTask);
                StopTask(_frontendTask);
                StopTask(_payloadServiceTask);

                return _kernelTask;
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
}
