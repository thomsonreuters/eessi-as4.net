using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Template Method Polling Base Class to expose the Polling mechanism over the Receivers
    /// </summary>
    /// <typeparam name="TIn">Incoming Message Type from the Polling LocationParameter</typeparam>
    /// <typeparam name="TOut">Out coming Message Type when the Message is Received</typeparam>
    public abstract class PollingTemplate<TIn, TOut>
    {
        protected abstract ILogger Logger { get; }
        protected abstract TimeSpan PollingInterval { get; }

        /// <summary>
        /// Start Polling to the given Target
        /// </summary>
        /// <param name="onMessage">Message Callback after the Message is received</param>
        /// <param name="cancellationToken"></param>
        protected void StartPolling(Func<TOut, CancellationToken, Task<MessagingContext>> onMessage, CancellationToken cancellationToken)
        {
            if (PollingInterval.Ticks <= 0)
            {
                throw new ConfigurationErrorsException("PollingInterval must be greater than zero");
            }

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    IEnumerable<TIn> messagesToPoll = GetMessagesToPoll(cancellationToken);

                    if (messagesToPoll.Any())
                    {
                        IEnumerable<Task> taskCollection = CreateTaskForEachMessage(messagesToPoll, onMessage, cancellationToken);
                        WaitForAllTasksToComplete(taskCollection, messagesToPoll);
                    }
                    else
                    {
                       Thread.Sleep(PollingInterval);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Trace("Cancellation requested; stopped polling.");
            }
            finally
            {
                // Make sure that pending items are released.
                ReleasePendingItems();
            }
        }

        private void WaitForAllTasksToComplete(IEnumerable<Task> taskCollection, IEnumerable<TIn> messagesToPoll)
        {
            try
            {
                try
                {
                    Task.WaitAll(taskCollection.ToArray());
                }
                catch (AggregateException err)
                {
                    err.Handle(e => e is TaskCanceledException);
                }
            }
            catch (Exception exception)
            {
                foreach (TIn message in messagesToPoll)
                {
                    HandleMessageException(message, exception);
                }
            }
        }

        /// <summary>
        /// Declaration to where the Message are and can be polled
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract IEnumerable<TIn> GetMessagesToPoll(CancellationToken cancellationToken);

        /// <summary>
        /// Describe what to do when a exception is thrown
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected abstract void HandleMessageException(TIn message, Exception exception);

        /// <summary>
        /// Declaration to the action that has to executed when a Message is received
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="messageCallback">Message Callback after the Message is received</param>
        /// <param name="token"></param>
        protected abstract void MessageReceived(
            TIn entity,
            Func<TOut, CancellationToken, Task<MessagingContext>> messageCallback,
            CancellationToken token);

        protected abstract void ReleasePendingItems();

        private IEnumerable<Task> CreateTaskForEachMessage(
            IEnumerable<TIn> messagesToPoll,
            Func<TOut, CancellationToken, Task<MessagingContext>> messageCallback,
            CancellationToken cancellationToken)
        {
            return messagesToPoll
                .Where(m => m != null)
                .Select(message => 
                    Task.Run(() => MessageReceived(message, messageCallback, cancellationToken))
                        .ContinueWith(LogInnerExceptions, TaskContinuationOptions.OnlyOnFaulted));
        }

        private static void LogInnerExceptions(Task task)
        {
            if (task.Exception?.InnerExceptions != null)
            {
                foreach (Exception ex in task.Exception?.InnerExceptions)
                {
                    LogManager.GetCurrentClassLogger().Fatal(ex.Message);
                    LogManager.GetCurrentClassLogger().Fatal(ex.StackTrace);
                }
            }
        }
    }
}