using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Repositories;
using NLog;
using NotSupportedException = System.NotSupportedException;

namespace Eu.EDelivery.AS4.Agents
{
    internal class RetryAgent : IAgent
    {
        private readonly IReceiver _receiver;
        private readonly TimeSpan _pollingInterval;
        private readonly Func<DatastoreContext> _createContext;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAgent"/> class.
        /// </summary>
        /// <param name="receiver">The receiver used to retrieve <see cref="RetryReliability"/> entities</param>
        /// <param name="pollingInterval">The interval in which the polling for retryable entities should happen</param>
        /// <param name="createContext">The factory creating a <see cref="DatastoreContext"/></param>
        public RetryAgent(
            IReceiver receiver,
            TimeSpan pollingInterval,
            Func<DatastoreContext> createContext)
        {
            _receiver = receiver;
            _pollingInterval = pollingInterval;
            _createContext = createContext;
        }

        /// <summary>
        /// Gets the agent configuration.
        /// </summary>
        /// <value>The agent configuration.</value>
        public AgentConfig AgentConfig { get; } = new AgentConfig("Retry Agent");

        /// <summary>
        /// Starts the specified agent.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public async Task Start(CancellationToken cancellation)
        {
            Logger.Debug(AgentConfig.Name + " Started");

            await Task.Factory.StartNew(
                () => _receiver.StartReceiving(OnReceived, cancellation),
                TaskCreationOptions.LongRunning);
        }

        private Task<MessagingContext> OnReceived(ReceivedMessage rm, CancellationToken ct)
        {
            try
            {
                if (rm is ReceivedEntityMessage rem && rem.Entity is RetryReliability rr)
                {
                    using (DatastoreContext ctx = _createContext())
                    {
                        var repo = new DatastoreRepository(ctx);
                        OnReceivedEntity(rr, repo);
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    throw new NotSupportedException(
                        $"Only {nameof(ReceivedEntityMessage)} implementations are allowed");
                }
            }
            catch (Exception ex)
            {
                // TODO: must the agent be stopped?
                Logger.Error(ex);
            }

            return Task.FromResult(
                new MessagingContext(rm, MessagingContextMode.Unknown));
        }

        private static void OnReceivedEntity(RetryReliability rr, DatastoreRepository repo)
        {
            (long refToEntityId, Entity entityType) = GetRefToEntityIdWithType(rr);
            Operation op = GetRefEntityOperation(repo, refToEntityId, entityType);

            Logger.Debug($"Retry on {entityType} {refToEntityId} with Operation={op}");

            if (op == Operation.ToBeRetried && rr.CurrentRetryCount < rr.MaxRetryCount)
            {
                var t = rr.RetryType;
                Operation updateOperation =
                    t == RetryType.Delivery ? Operation.ToBeDelivered :
                    t == RetryType.Notification ? Operation.ToBeNotified : 
                    t == RetryType.Send ? Operation.ToBeSent : Operation.NotApplicable;

                Logger.Debug($"({rr.RetryType}) Update for retry, set Operation={updateOperation}");
                UpdateRefEntityOperation(repo, refToEntityId, entityType, updateOperation);

                Logger.Debug(
                    $"({rr.RetryType}) Update retry to try again " +
                    $"{{CurrentRetry={rr.CurrentRetryCount + 1}, Status=Pending, LastRetryTime=Now}}");

                repo.UpdateRetryReliability(rr.Id, r =>
                {
                    r.CurrentRetryCount = r.CurrentRetryCount + 1;
                    r.LastRetryTime = DateTimeOffset.Now;
                });

            }
            else if (rr.CurrentRetryCount >= rr.MaxRetryCount)
            {
                Logger.Debug($"({rr.RetryType}) Retry operation is completed, no new retries will happen");
                Logger.Debug($"({rr.RetryType}) Update referenced entity {{Operation=DeadLettered}}");
                Logger.Debug($"({rr.RetryType}) Update retry {{Status=Completed}}");

                UpdateRefEntityOperation(repo, refToEntityId, entityType, Operation.DeadLettered);
                repo.UpdateRetryReliability(rr.Id, r => r.Status = RetryStatus.Completed);
            }
        }

        private static (long, Entity) GetRefToEntityIdWithType(RetryReliability r)
        {
            if (r.RefToInMessageId.HasValue)
            {
                return (r.RefToInMessageId.Value, Entity.InMessage);
            }

            if (r.RefToOutMessageId.HasValue)
            {
                return (r.RefToOutMessageId.Value, Entity.OutMessage);
            }

            if (r.RefToInExceptionId.HasValue)
            {
                return (r.RefToInExceptionId.Value, Entity.InException);
            }

            if (r.RefToOutExceptionId.HasValue)
            {
                return (r.RefToOutExceptionId.Value, Entity.OutException);
            }

            throw new InvalidOperationException(
                "Invalid 'RetryReliability' record: requries a reference to In/Out Messages/Exceptions");
        }

        private enum Entity { InMessage, OutMessage, InException, OutException }

        private static Operation GetRefEntityOperation(DatastoreRepository repo, long id, Entity type)
        {
            switch (type)
            {
                case Entity.InMessage:
                    return repo.GetInMessageData(id, m => m.Operation);
                case Entity.OutMessage:
                    return repo.GetOutMessageData(id, m => m.Operation);
                case Entity.InException:
                    return repo.GetInExceptionData(id, ex => ex.Operation);
                case Entity.OutException:
                    return repo.GetOutExceptionData(id, ex => ex.Operation);
                default:
                    throw new ArgumentOutOfRangeException(paramName: nameof(type), actualValue: type, message: null);
            }
        }

        private static void UpdateRefEntityOperation(DatastoreRepository repo, long id, Entity type, Operation o)
        {
            switch (type)
            {
                case Entity.InMessage:
                    repo.UpdateInMessage(id, m => m.Operation = o);
                    break;
                case Entity.OutMessage:
                    repo.UpdateOutMessage(id, m => m.Operation = o);
                    break;
                case Entity.InException:
                    repo.UpdateInException(id, ex => ex.Operation = o);
                    break;
                case Entity.OutException:
                    repo.UpdateOutException(id, ex => ex.Operation = o);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(paramName: nameof(type), actualValue: type, message: null);
            }
        }

        /// <summary>
        /// Stops this agent.
        /// </summary>
        public void Stop()
        {
            _receiver.StopReceiving();
        }
    }
}
