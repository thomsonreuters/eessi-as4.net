using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Describes the condition between <see cref="AS4Message" />
    /// </summary>
    public class ConditionalStep : IStep
    {
        private readonly Func<InternalMessage, bool> _condition;
        private readonly IStep _first;
        private readonly IStep _second;

        public async Task<StepResult> ExecuteAsync(AS4Message message, CancellationToken cancellationToken)
        {
            return await Task.Run(() => new StepResult(), cancellationToken);

            // return await (_condition(msg) ? _first : _second).TransmitMessageAsync(msg, ct);
        }

        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ConditionalStep(Func<InternalMessage, bool> condition, IStep first, IStep second)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (first == null)
                throw new ArgumentNullException(nameof(first));

            if (second == null)
                throw new ArgumentNullException(nameof(second));

            this._condition = condition;
            this._first = first;
            this._second = second;
        }
    }
}