using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    internal static class PollingService
    {
        /// <summary>
        /// Poll at a given <paramref name="directoryPath"/>.
        /// </summary>
        /// <param name="directoryPath">The file path at which the the polling mechanism will happen.</param>
        public static Task<IEnumerable<FileInfo>> PollUntilPresentAsync(string directoryPath)
        {
            return PollUntilPresentAsync(directoryPath, fs => fs.Any());
        }

        /// <summary>
        /// Poll at a given <paramref name="directoryPath"/> for a given <paramref name="predicate"/> 
        /// </summary>
        /// <param name="directoryPath">The file path at which the the polling mechanism will happen.</param>
        /// <param name="predicate">The filter to select only a portion of the files at the given file path.</param>
        public static Task<IEnumerable<FileInfo>> PollUntilPresentAsync(
            string directoryPath,
            Func<IEnumerable<FileInfo>, bool> predicate)
        {
            return PollUntilPresentAsync(directoryPath, predicate, TimeSpan.FromSeconds(40));
        }

        /// <summary>
        /// Poll at a given <paramref name="directoryPath"/> for a given <paramref name="predicate"/> 
        /// until the <paramref name="timeout"/> expires.
        /// </summary>
        /// <param name="directoryPath">The file path at which the the polling mechanism will happen.</param>
        /// <param name="predicate">The filter to select only a portion of the files at the given file path.</param>
        /// <param name="timeout">The duration until the polling throws with a <see cref="TimeoutException"/>.</param>
        public static Task<IEnumerable<FileInfo>> PollUntilPresentAsync(
            string directoryPath,
            Func<IEnumerable<FileInfo>, bool> predicate,
            TimeSpan timeout)
        {
            return PollUntilPresentAsync(
                () =>
                {
                    IEnumerable<FileInfo> files =
                        Directory.GetFiles(directoryPath)
                                 .Select(s => new FileInfo(s));

                    Console.WriteLine($@"Poll until present at: {directoryPath}");
                    foreach (FileInfo f in files)
                    {
                        Console.WriteLine($@"Found file at {directoryPath}: {f.Name}");
                    }

                    return Task.FromResult(files);
                },
                predicate,
                timeout);
        }

        /// <summary>
        /// Poll using a given <paramref name="pollAsync"/> funciton for a given <paramref name="predicate"/> 
        /// until the <paramref name="timeout"/> expires.
        /// </summary>
        /// <param name="pollAsync">The function to execute during the polling mechanimsm.</param>
        /// <param name="predicate">The filter to select only a portion of the resulted values.</param>
        /// <param name="timeout">The duration until the polling throws with a <see cref="TimeoutException"/>.</param>
        public static Task<TResult> PollUntilPresentAsync<TResult>(
            Func<Task<TResult>> pollAsync,
            Func<TResult, bool> predicate,
            TimeSpan timeout)
        {
            IObservable<TResult> polling =
                Observable.Create<TResult>(async o =>
                {
                    TResult r = await pollAsync();
                    IObservable<TResult> observable =
                        predicate(r)
                            ? Observable.Return(r)
                            : Observable.Throw<TResult>(new Exception());

                    return observable.Subscribe(o);
                });

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            return Observable
                   .Timer(TimeSpan.FromSeconds(1))
                   .SelectMany(_ => polling)
                   .Retry()
                   .ToTask(cts.Token);
        }
    }
}
