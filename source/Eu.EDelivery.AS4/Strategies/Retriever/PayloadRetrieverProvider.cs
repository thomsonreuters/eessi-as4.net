using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// Class to provide <see cref="IPayloadRetriever"/> implementations
    /// </summary>
    internal class PayloadRetrieverProvider : IPayloadRetrieverProvider
    {
        private readonly ICollection<PayloadStrategyEntry> _entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadRetrieverProvider"/> class. 
        /// Create a new Provider with empty <see cref="IPayloadRetriever"/> implementations
        /// </summary>
        internal PayloadRetrieverProvider()
        {
            _entries = new Collection<PayloadStrategyEntry>();
        }

        /// <summary>
        /// Get a specific Payload Retriever for a given Payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public IPayloadRetriever Get(Model.Common.Payload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            PayloadStrategyEntry entry = _entries.FirstOrDefault(e => e.Condition(payload));

            if (entry?.Retriever == null)
            {
                throw new KeyNotFoundException(
                    $"No {nameof(IPayloadRetriever)} implementation found for payload {payload.Id}");
            }

            return entry.Retriever;
        }

        /// <summary>
        /// Accept a new <see cref="IPayloadRetriever"/>
        /// </summary>
        /// <param name="condition">Condition which couples the kind of Payload with a <see cref="IPayloadRetriever"/> implementation</param>
        /// <param name="retriever"></param>
        public void Accept(Func<Model.Common.Payload, bool> condition, IPayloadRetriever retriever)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (retriever == null)
            {
                throw new ArgumentNullException(nameof(retriever));
            }

            var entry = new PayloadStrategyEntry(condition, retriever);
            _entries.Add(entry);
        }

        /// <summary>
        /// Helper class to register the <see cref="IPayloadRetriever"/> implementation
        /// </summary>
        private class PayloadStrategyEntry 
        {
            public Func<Model.Common.Payload, bool> Condition { get; }
            public IPayloadRetriever Retriever { get; }

            public PayloadStrategyEntry(Func<Model.Common.Payload, bool> condition, IPayloadRetriever retriever)
            {
                Condition = condition;
                Retriever = retriever;
            }
        }
    }

    /// <summary>
    /// Interface for the Payload Provider
    /// Used for mocking
    /// </summary>
    public interface IPayloadRetrieverProvider
    {
        /// <summary>
        /// Get a specific Payload Retriever for a given Payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        IPayloadRetriever Get(Model.Common.Payload payload);

        /// <summary>
        /// Accept a new <see cref="IPayloadRetriever"/>
        /// </summary>
        /// <param name="condition">Condition which couples the kind of Payload with a <see cref="IPayloadRetriever"/> implementation</param>
        /// <param name="retriever"></param>
        void Accept(Func<Model.Common.Payload, bool> condition, IPayloadRetriever retriever);
    }
}