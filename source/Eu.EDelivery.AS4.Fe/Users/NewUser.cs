namespace Eu.EDelivery.AS4.Fe.Users
{
    /// <summary>
    /// New user
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Users.User" />
    public class NewUser : User
    {
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }
    }
}