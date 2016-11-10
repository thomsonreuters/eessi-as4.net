using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Service : IEquatable<Service>
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public Service()
        {
            this.Name = Constants.Namespaces.TestService;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Service other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.Type, other.Type, StringComparison.OrdinalIgnoreCase);
        }
    }
}