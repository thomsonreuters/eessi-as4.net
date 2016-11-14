using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Interface <see cref="IStep" /> to describe a single step to execute
    /// </summary>
    public interface IStep
    {
        Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Versioned interface for the <see cref="IStep"/> interface
    /// to make the <see cref="IStep"/> implementation configurable
    /// </summary>
    public interface IConfigStep : IStep
    {
        /// <summary>
        /// Configure the step with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        void Configure(IDictionary<string, string> properties);
    }
}