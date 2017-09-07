using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Authentication;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    public class FileSystemService
    {
        /// <summary>
        /// Cleanup files in a given Directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="predicateFile">The predicate File.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public void CleanUpFiles(string directory, Func<string, bool> predicateFile = null)
        {
            EnsureDirectory(directory);

            Console.WriteLine($@"Deleting files at location: {directory}");

            foreach (string file in Directory.EnumerateFiles(directory))
            {
                if (predicateFile == null || predicateFile(file))
                {
                    WhileTimeOutTry(retryCount: 10, retryAction: () => File.Delete(file));
                }
            }
        }

        public void RemoveDirectory(string directory)
        {
            EnsureDirectory(directory);
            WhileTimeOutTry(5, retryAction: () => Directory.Delete(directory, recursive: true));
        }

        private static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private static void WhileTimeOutTry(int retryCount, Action retryAction)
        {
            var count = 0;

            while (count < retryCount)
            {
                try
                {
                    retryAction();
                    return;
                }
                catch (IOException exception)
                {
                    Console.WriteLine(exception);
                    count++;

                    Thread.Sleep(TimeSpan.FromSeconds(3));
                }
            }

            throw new TimeoutException("Failed to perform the operation in the specified timeframe.");
        }
    }
}
