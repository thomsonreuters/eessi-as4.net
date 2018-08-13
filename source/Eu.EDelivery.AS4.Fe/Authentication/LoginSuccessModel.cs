using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    /// <summary>
    /// Model containing successful login data
    /// </summary>
    public class LoginSuccessModel
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}