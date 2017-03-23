using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.UnitTests.Model.PMode
{
    /// <summary>
    /// Valid instance of the <see cref="SendingProcessingMode" />
    /// </summary>
    public class ValidStubSendingPMode : SendingProcessingMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidStubSendingPMode"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public ValidStubSendingPMode(string id)
        {
            Id = id;
            PushConfiguration = new PushConfiguration {Protocol = new Protocol {Url = "http://127.0.0.1/msh"}};
            PullConfiguration = new PullConfiguration {Protocol = new Protocol {Url = "http://127.0.0.1/msh"}};
            Security = new AS4.Model.PMode.Security
            {
                Signing =
                    new Signing
                    {
                        PrivateKeyFindValue = "My",
                        PrivateKeyFindType = X509FindType.FindBySubjectName,
                        Algorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                        HashFunction = "http://www.w3.org/2001/04/xmlenc#sha256"
                    }
            };
        }
    }
}