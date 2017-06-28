using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// Class to provide <see cref="IPayloadRetriever"/> implementations
    /// </summary>
    public class PayloadRetrieverProvider : IPayloadRetrieverProvider
    {
        private readonly ICollection<PayloadStrategyEntry> _entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadRetrieverProvider"/> class. 
        /// Create a new Provider with empty <see cref="IPayloadRetriever"/> implementations
        /// </summary>
        public PayloadRetrieverProvider()
        {
            this._entries = new Collection<PayloadStrategyEntry>();
        }

        /// <summary>
        /// Get a specific Payload Retriever for a given Payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public IPayloadRetriever Get(Model.Common.Payload payload)
        {
            PayloadStrategyEntry entry = _entries.FirstOrDefault(e => e.Condition(payload));

            if (entry?.Retriever == null)
            {
                throw new KeyNotFoundException($"No Payload Retriever found for Payload {payload.Id}");
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
            var entry = new PayloadStrategyEntry(condition, retriever);
            this._entries.Add(entry);
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
                this.Condition = condition;
                this.Retriever = retriever;
            }
        }
    }

    /// <summary>
    /// Interface for the Payload Provider
    /// Used for mocking
    /// </summary>
    public interface IPayloadRetrieverProvider
    {
        IPayloadRetriever Get(Model.Common.Payload payload);
        void Accept(Func<Model.Common.Payload, bool> condition, IPayloadRetriever retriever);
    }
}