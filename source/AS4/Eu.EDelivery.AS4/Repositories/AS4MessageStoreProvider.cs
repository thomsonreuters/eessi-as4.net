using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Repositories
{
    public class AS4MessageBodyRetrieverProvider
    {
        private readonly ICollection<AS4MessageRepositoryEntry> _repositories = new Collection<AS4MessageRepositoryEntry>();

        public void Accept(Func<string, bool> condition, IAS4MessageBodyRetriever retriever)
        {
            _repositories.Add(new AS4MessageRepositoryEntry(condition, retriever));
        }

        public IAS4MessageBodyRetriever Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var result = _repositories.FirstOrDefault(s => s.Condition(key));

            if (result == null)
            {
                throw new AS4Exception($"No registered IAS4MessageRepository found for {key}");
            }

            return result.Store;
        }

        private sealed class AS4MessageRepositoryEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AS4MessageRepositoryEntry"/> class.
            /// </summary>
            public AS4MessageRepositoryEntry(Func<string, bool> condition, IAS4MessageBodyRetriever store)
            {
                Condition = condition;
                Store = store;
            }

            public Func<string, bool> Condition { get; }
            public IAS4MessageBodyRetriever Store { get; }
        }
    }
}
