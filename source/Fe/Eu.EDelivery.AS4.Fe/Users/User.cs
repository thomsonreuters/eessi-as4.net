using System.Collections.Generic;
using System.Security.Claims;
using Eu.EDelivery.AS4.Fe.Authentication;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Fe.Users
{
    /// <summary>
    /// Class representing a user
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is admin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is admin; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdmin
        {
            get => Claims.Contains(Roles.Admin);
            set
            {
                if (value) Claims.Add(Roles.Admin);
                else Claims.Remove(Roles.Admin);
            }
        }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [JsonIgnore]
        public List<string> Claims { get; set; } = new List<string>();
    }
}