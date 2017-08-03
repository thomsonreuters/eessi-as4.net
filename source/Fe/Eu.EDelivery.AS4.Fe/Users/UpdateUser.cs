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
        /// Gets or sets a value indicating whether this instance is admin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is admin; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdmin { get; set; }
    }
}