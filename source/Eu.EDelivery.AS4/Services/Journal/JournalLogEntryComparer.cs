using System;
using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Services.Journal
{
    /// <summary>
    /// Comparer implementation to distinguish equal <see cref="JournalLogEntry"/> instances.
    /// </summary>
    internal class JournalLogEntryComparer : IEqualityComparer<JournalLogEntry>
    {
        public static IEqualityComparer<JournalLogEntry> ByEbmsMessageId = new JournalLogEntryComparer();

        private JournalLogEntryComparer() { }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(JournalLogEntry x, JournalLogEntry y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(x.EbmsMessageId, y.EbmsMessageId);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.</exception>
        public int GetHashCode(JournalLogEntry obj)
        {
            return obj.GetHashCode();
        }
    }
}