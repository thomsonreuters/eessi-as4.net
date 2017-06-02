using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.UnitTests.Model.PMode
{
    /// <summary>
    /// Valid instance of the <see cref="SendingProcessingMode" />
    /// </summary>
    public class ValidSendingPModeFactory : SendingProcessingMode
    {
        /// <summary>
        /// Create a valid <see cref="SendingProcessingMode"/> instance.
        /// </summary>        
        /// <param name="id">Optional PMode Id</param>
        public SendingProcessingMode Create(string id = "default-id")
        {
            return new SendingProcessingMode
            {
                Id = id,
                PushConfiguration = new PushConfiguration {Protocol = new Protocol {Url = "http://127.0.0.1/msh"}},
                PullConfiguration = new PullConfiguration {Protocol = new Protocol {Url = "http://127.0.0.1/msh"}},
                Security =
                    new AS4.Model.PMode.Security
                    {
                        Signing =
                            new Signing
                            {
                                PrivateKeyFindValue = "My",
                                PrivateKeyFindType = X509FindType.FindBySubjectName,
                                Algorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                                HashFunction = "http://www.w3.org/2001/04/xmlenc#sha256"
                            },
                        Encryption = new Encryption
                        {
                            PublicKeyFindValue = "Ignored"
                        }
                    }
            };
        }
    }
}