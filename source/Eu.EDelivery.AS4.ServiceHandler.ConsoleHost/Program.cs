using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using log4net;

namespace Eu.EDelivery.AS4.ServiceHandler.ConsoleHost
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void Main()
        {
            Console.SetWindowSize(Console.LargestWindowWidth, Console.WindowHeight);

            ShowHelp();

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
                    key = Console.ReadKey();

                    switch (key.Key)
                    {
                        case ConsoleKey.C:
                            Console.Clear();
                            ShowHelp();
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
                } while (key.Key != ConsoleKey.Q);

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
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
            finally
            {
                kernel?.Dispose();
                Config.Instance.Dispose();
            }

            Console.ReadLine();
        }

        private static void ShowHelp()
        {
            WriteLine("\nAS4.NET v" + Assembly.GetExecutingAssembly().GetName().Version + "\n"
                      + "\nThe following commands are available while the AS4.NET MSH is running:"
                      + "\n c\tClears the screen"
                      + "\n q\tQuits the application"
                      + "\n r\tRestarts the application"
                      + "\n");
        }

        private static void WriteLine(string msg)
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(msg);
            Console.ForegroundColor = temp;
        }

        private static Kernel CreateKernel()
        {
            try
            {
                return Kernel.CreateFromSettings(@"config\settings.xml");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private sealed class AS4ComponentLifecycle
        {
            private readonly CancellationTokenSource _cancellation;

            private readonly Kernel _kernel;
            private Task _kernelTask;


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
            }

            private Task StartKernel()
            {
                Task task = _kernel.StartAsync(_cancellation.Token);

                task.ContinueWith(
                    x =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.Fatal(x.Exception?.ToString());
                    },
                    TaskContinuationOptions.OnlyOnFaulted);

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
                    Logger.Error(ex);
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
