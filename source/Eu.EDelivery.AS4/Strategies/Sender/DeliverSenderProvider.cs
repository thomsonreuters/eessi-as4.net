using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Class to provide <see cref="IDeliverSender" /> implementations
    /// based on a given condition
    /// </summary>
    internal class DeliverSenderProvider : IDeliverSenderProvider
    {
        private readonly ICollection<DeliverSenderEntry> _senders;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverSenderProvider" /> class.
        /// Create a new <see cref="DeliverSenderProvider" />
        /// to select the provide the right <see cref="IDeliverSender" /> implementation
        /// </summary>
        internal DeliverSenderProvider()
        {
            _senders = new Collection<DeliverSenderEntry>();
        }

        /// <summary>
        /// Accept a given <paramref name="sender" /> for a given <paramref name="condition" />
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="sender"></param>
        public void Accept(Func<string, bool> condition, Func<IDeliverSender> sender)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            _senders.Add(new DeliverSenderEntry(condition, sender));
        }

        /// <summary>
        /// Get the right <see cref="IDeliverSender" /> implementation
        /// for a given <paramref name="operationMethod" />
        /// </summary>
        /// <param name="operationMethod"></param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public IDeliverSender GetDeliverSender(string operationMethod)
        {
            if (operationMethod == null)
            {
                throw new ArgumentNullException(nameof(operationMethod));
            }

            DeliverSenderEntry entry = _senders.FirstOrDefault(s => s.Condition(operationMethod));

            if (entry?.Sender == null)
            {
                throw new KeyNotFoundException($"No Deliver Sender found for a given {operationMethod} Operation Method");
            }

            return entry.Sender();
        }

        /// <summary>
        /// Value Object to define a entry for the <see cref="IDeliverSenderProvider" />
        /// </summary>
        private class DeliverSenderEntry
        {
            public DeliverSenderEntry(Func<string, bool> condition, Func<IDeliverSender> sender)
            {
                Condition = condition;
                Sender = sender;
            }

            public Func<string, bool> Condition { get; }

            public Func<IDeliverSender> Sender { get; }
        }
    }

    /// <summary>
    /// Interface to define the <see cref="IDeliverSender" /> selection
    /// </summary>
    public interface IDeliverSenderProvider
    {
        /// <summary>
        /// Accept a given <paramref name="sender" /> for a given <paramref name="condition" />
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="sender"></param>
        void Accept(Func<string, bool> condition, Func<IDeliverSender> sender);

        /// <summary>
        /// Get the right <see cref="IDeliverSender" /> implementation
        /// for a given <paramref name="operationMethod" />
        /// </summary>
        /// <param name="operationMethod"></param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        IDeliverSender GetDeliverSender(string operationMethod);
    }
}