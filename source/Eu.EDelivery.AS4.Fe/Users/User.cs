using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Users
{
    /// <summary>
    ///     Class representing a user
    /// </summary>
    public class User
    {
        /// <summary>
        ///     Gets or sets the name of the user.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the claims.
        /// </summary>
        /// <value>
        ///     The claims.
        /// </value>
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}
