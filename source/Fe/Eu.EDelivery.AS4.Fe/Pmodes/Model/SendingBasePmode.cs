using Eu.EDelivery.AS4.Fe.Monitor.Model;
using Eu.EDelivery.AS4.Model.PMode;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.Fe.Pmodes.Model
{
    /// <summary>
    /// Class to hold a sending pmode
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Pmodes.Model.BasePmode{Eu.EDelivery.AS4.Model.PMode.SendingProcessingMode}" />
    public class SendingBasePmode : BasePmode<SendingProcessingMode>
    {
        private SendingProcessingMode pmode;

        /// <summary>
        /// Gets or sets the pmode.
        /// </summary>
        /// <value>
        /// The pmode.
        /// </value>
        public override SendingProcessingMode Pmode
        {
            get { return pmode; }
            set
            {
                pmode = value;
                if (value?.Security?.Encryption != null && value.Security.Encryption.PublicKeyInformation is JObject json)
                {
                    if (json["certificate"] != null) value.Security.Encryption.PublicKeyInformation = json.ToObject<PublicKeyCertificate>();
                    else value.Security.Encryption.PublicKeyInformation = json.ToObject<PublicKeyFindCriteria>();
                }

                if (value?.MepBinding == MessageExchangePatternBinding.Pull)
                {
                    value.PushConfiguration = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether push configuration is enabled.
        /// Setting this to false will result in the PushConfiguration node being removed from the xml.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is push configuration enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDynamicDiscoveryEnabled
        {
            get
            {
                return Pmode?.PushConfiguration == null;
            }
            set
            {
                if (value)
                {
                    Pmode.PushConfiguration = null;
                }
                else
                {
                    pmode.DynamicDiscovery = null;
                }
            }
        }
    }
}