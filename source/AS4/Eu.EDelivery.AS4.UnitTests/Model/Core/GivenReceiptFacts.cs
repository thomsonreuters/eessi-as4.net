using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using FsCheck.Xunit;
using CryptoReference = System.Security.Cryptography.Xml.Reference;
using Reference = Eu.EDelivery.AS4.Model.Core.Reference;

namespace Eu.EDelivery.AS4.UnitTests.Model.Core
{
    /// <summary>
    /// Testing <see cref="Receipt"/>
    /// </summary>
    public class GivenReceiptFacts
    {
        private const string CommonUri = "reference-uri";

        [Property]
        public bool VerifyNonRepudiationsReferences(byte[] nonRepudiationHash, byte[] userReferences)
        {
            return NRRReceipt(nonRepudiationHash)
                .VerifyNonRepudiations(UserReferences(userReferences))
                .Equals(nonRepudiationHash.SequenceEqual(userReferences));
        }

        private static Receipt NRRReceipt(byte[] referenceHash)
        {
            return new Receipt
            {
                NonRepudiationInformation =
                    new NonRepudiationInformation
                    {
                        MessagePartNRInformation =
                            new List<MessagePartNRInformation>
                            {
                                new MessagePartNRInformation
                                {
                                    Reference = new Reference {URI = CommonUri, DigestValue = referenceHash}
                                }
                            }
                    }
            };
        }

        private static IEnumerable<CryptoReference> UserReferences(byte[] referenceHash)
        {
            return new List<CryptoReference>
            {
                new CryptoReference {Uri = CommonUri, DigestValue = referenceHash}
            };
        }
    }
}
