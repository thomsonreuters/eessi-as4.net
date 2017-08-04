using System.Collections;
using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Users
{
    /// <summary>
    /// Update user object
    /// </summary>
    public class UpdateUser
    {
        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}