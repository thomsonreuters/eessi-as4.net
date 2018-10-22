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
    public class NotifySenderProvider : INotifySenderProvider
    {
        private readonly ICollection<NotifySenderEntry> _senders;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifySenderProvider" /> class.
        /// Create a new <see cref="NotifySenderProvider" />
        /// to select the provide the right <see cref="INotifySender" /> implementation
        /// </summary>
        internal NotifySenderProvider()
        {
            _senders = new Collection<NotifySenderEntry>();
        }

        /// <summary>
        /// Accept a given <paramref name="sender" /> for a given <paramref name="condition" />
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="sender"></param>
        public void Accept(Func<string, bool> condition, Func<INotifySender> sender)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            _senders.Add(new NotifySenderEntry(condition, sender));
        }

        /// <summary>
        /// Get the right <see cref="INotifySender" /> implementation
        /// for a given <paramref name="operationMethod" />
        /// </summary>
        /// <param name="operationMethod"></param>  
        /// <returns></returns>
        public INotifySender GetNotifySender(string operationMethod)
        {
            if (operationMethod == null)
            {
                throw new ArgumentNullException(nameof(operationMethod));
            }

            NotifySenderEntry entry = _senders.FirstOrDefault(s => s.Condition(operationMethod));

            if (entry?.Sender == null)
            {
                throw new KeyNotFoundException(
                    $"No {nameof(INotifySender)} impelemtation found for Operation Method \'{operationMethod}\'. " + 
                    "Please check if the configuration in the Sending or Receiving PMode is correct");
            }

            return entry.Sender();
        }

        /// <summary>
        /// Value Object to define a entry for the <see cref="INotifySender" />
        /// </summary>
        private class NotifySenderEntry
        {
            public NotifySenderEntry(Func<string, bool> condition, Func<INotifySender> sender)
            {
                Condition = condition;
                Sender = sender;
            }

            public Func<string, bool> Condition { get; }

            public Func<INotifySender> Sender { get; }
        }
    }

    /// <summary>
    /// Interface to define the <see cref="INotifySender" /> selection
    /// </summary>
    public interface INotifySenderProvider
    {
        void Accept(Func<string, bool> condition, Func<INotifySender> sender);

        INotifySender GetNotifySender(string operationMethod);
    }
}