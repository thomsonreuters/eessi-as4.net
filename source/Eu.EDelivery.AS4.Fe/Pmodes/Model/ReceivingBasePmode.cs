using System;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.PMode;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.Fe.Pmodes.Model
{

    public class ReceivingBasePmode : BasePmode<ReceivingProcessingMode>
    {
        private ReceivingProcessingMode pmode;

        /// <summary>
        /// Gets or sets the pmode.
        /// </summary>
        /// <value>
        /// The pmode.
        /// </value>
        public override ReceivingProcessingMode Pmode
        {
            get => pmode;
            set
            {
                pmode = value;
                if (value?.Security?.Decryption == null || !(value.Security.Decryption.DecryptCertificateInformation is JObject json))
                {
                    return;
                }

                value.Security.Decryption.DecryptCertificateInformation = json.ToObject<CertificateFindCriteria>();

                if (value?.ReplyHandling != null)
                {
                    if (value.ReplyHandling.ReplyPattern != ReplyPattern.PiggyBack)
                    {
                        value.ReplyHandling.PiggyBackReliability = null;
                    }

                    if (value.ReplyHandling.ReplyPattern != ReplyPattern.Callback)
                    {
                        value.ReplyHandling.ResponseConfiguration = null;
                    }

                    if (value.ReplyHandling.ResponseConfiguration?.TlsConfiguration != null
                        && value.ReplyHandling.ResponseConfiguration?.TlsConfiguration?.ClientCertificateInformation is JObject clientCert)
                    {
                        if (clientCert["certificate"] != null)
                        {
                            value.ReplyHandling.ResponseConfiguration.TlsConfiguration.ClientCertificateInformation =
                                clientCert.ToObject<PrivateKeyCertificate>();
                        }
                        else
                        {
                            string jsonFindType =
                                clientCert["clientCertificateFindType"] != null
                                    ? clientCert["clientCertificateFindType"].Value<string>()
                                    : ClientCertificateReference.DefaultCertificateFindType.ToString();

                            string jsonFindValue =
                                clientCert["clientCertificateFindValue"] != null
                                    ? clientCert["clientCertificateFindValue"].Value<string>()
                                    : String.Empty;

                            X509FindType certFindType =
                                Enum.TryParse(jsonFindType, out X509FindType result)
                                    ? result
                                    : ClientCertificateReference.DefaultCertificateFindType;

                            value.ReplyHandling.ResponseConfiguration.TlsConfiguration.ClientCertificateInformation =
                                new ClientCertificateReference
                                {
                                    ClientCertificateFindType = certFindType,
                                    ClientCertificateFindValue = jsonFindValue
                                };
                        }
                    }
                }

                if (value?.ReplyHandling?.ResponseSigning?.SigningCertificateInformation != null
                    && value.ReplyHandling.ResponseSigning.SigningCertificateInformation is JObject signing)
                {
                    if (signing["certificate"] != null
                        && signing["password"] != null)
                    {
                        value.ReplyHandling.ResponseSigning.SigningCertificateInformation =
                            signing.ToObject<PrivateKeyCertificate>();
                    }
                    else if (signing["certificateFindType"] != null
                             && signing["certificateFindValue"] != null)
                    {
                        value.ReplyHandling.ResponseSigning.SigningCertificateInformation =
                            signing.ToObject<CertificateFindCriteria>();
                    }
                }
            }
        }
    }
}