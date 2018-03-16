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
            get => pmode;
            set
            {
                pmode = value;

                if (value?.MepBinding == MessageExchangePatternBinding.Pull)
                {
                    value.PushConfiguration = null;
                }
                else if (value.PushConfiguration?.TlsConfiguration != null 
                    && value.PushConfiguration.TlsConfiguration.ClientCertificateInformation is JObject clientCert)
                {
                    if (clientCert["certificate"] != null)
                    {
                        value.PushConfiguration.TlsConfiguration.ClientCertificateInformation =
                            clientCert.ToObject<PrivateKeyCertificate>();
                    }
                    else
                    {
                        value.PushConfiguration.TlsConfiguration.ClientCertificateInformation =
                            clientCert.ToObject<ClientCertificateReference>();
                    }
                    
                }

                if (value?.Security?.Encryption != null 
                    && value.Security.Encryption.EncryptionCertificateInformation is JObject encryptCert)
                {
                    if (encryptCert["certificate"] != null)
                    {
                        value.Security.Encryption.EncryptionCertificateInformation =
                            encryptCert.ToObject<PublicKeyCertificate>();
                    }
                    else
                    {
                        value.Security.Encryption.EncryptionCertificateInformation = encryptCert.ToObject<CertificateFindCriteria>();
                    }
                }

                if (value?.Security?.Signing?.SigningCertificateInformation != null 
                    && (value.Security.Signing.SigningCertificateInformation is JObject signing))
                {
                    value.Security.Signing.SigningCertificateInformation = signing.ToObject<CertificateFindCriteria>();
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