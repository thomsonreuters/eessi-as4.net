using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExponentialIntervalReceiver
{
    class Program
    {
        private static ExponentialIntervalReceiver Receiver;

        // How can we support piggybacking; we should be able to prevent to 'pull' messages for a given mpc if the previous message was not completely processed.
        // configure receiver to support singlethreading / multithreading ?
        // always singlethreaded per pmode ?
        static void Main(string[] args)
        {
            var data = new[]
            {
                new PModeInformation("PMode1", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10)),
                new PModeInformation("PMode2", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(20)),
                new PModeInformation("PMode3", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(20)),
                new PModeInformation("PMode4", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(12)),
                new PModeInformation("PMode5", TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(60)),
                new PModeInformation("PMode6", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30)),
                new PModeInformation("PMode7", TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(12))
            };

            Receiver = new ExponentialIntervalReceiver(data);

            Console.WriteLine("Starting interval-pmode-receiver with following data:");
            foreach (var p in data)
            {
                Console.WriteLine(p);
            }

            Console.WriteLine();
            Console.WriteLine("Start!");

            Receiver.Start(OnPModeReceived);

            Console.ReadLine();

            Console.WriteLine("Stopping ...");

            Receiver.Stop();

            Console.WriteLine("Stopped");

            Console.WriteLine("RunHistory:");

            var numberOfRunsPerPMode = RunHistory.GroupBy(h => h.PMode);

            foreach (var item in numberOfRunsPerPMode)
            {
                Console.WriteLine($"{item.Key}: executed {item.Count()} times");
            }

            Console.ReadLine();

            var groupedRunHistory = RunHistory.GroupBy(h => h.RunDate);

            foreach (var group in groupedRunHistory)
            {
                Console.WriteLine($"Execution Date = {group.Key}");

                foreach (var item in group)
                {
                    Console.WriteLine($"\t{item}");
                }
            }

           

            Console.ReadLine();
        }

        private static readonly Random Randomizer = new Random();

        private static readonly List<RunInfoItem> RunHistory = new List<RunInfoItem>();

        private static void OnPModeReceived(PModeInformation pmode)
        {
            bool result = Randomizer.Next(13) % 2 == 0;

            Console.WriteLine($"Messages received for pmode {pmode.Name} = {result}");

            RunHistory.Add(new RunInfoItem(pmode.Name, DateTime.Now, result));

            Thread.Sleep(1500);

            Receiver.Reconfigure(new ExecuteResult(pmode.Name, result));
        }

        private class RunInfoItem
        {
            public string PMode { get; }
            public DateTime RunDate { get; }
            public bool Success { get; }

            public RunInfoItem(string pmode, DateTime runDate, bool success)
            {
                PMode = pmode;
                RunDate = runDate;
                Success = success;
            }

            public override string ToString()
            {
                return $"{PMode} - success:{Success}";
            }
        }
    }
}
