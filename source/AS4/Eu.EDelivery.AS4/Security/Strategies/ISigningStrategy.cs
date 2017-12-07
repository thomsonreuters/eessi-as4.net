using System.Collections;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    public interface ISigningStrategy
    {
        /// <summary>
        /// Get the signed references from the Signature
        /// </summary>
        /// <returns></returns>
        ArrayList GetSignedReferences();

        /// <summary>
        /// Gets the full security XML element.
        /// </summary>
        /// <param name="securityElement"></param>
        void AppendSignature(XmlElement securityElement);

        /// <summary>
        /// Sign the Signature of the <see cref="ISigningStrategy"/>
        /// </summary>
        void SignSignature();

        /// <summary>
        /// Verify the Signature of the <see cref="ISigningStrategy"/>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        bool VerifySignature(VerifySignatureConfig options);

        /// <summary>
        /// Gets the security token reference used to sign the <see cref="AS4Message"/>.
        /// </summary>
        /// <value>The security token reference.</value>
        SecurityTokenReference SecurityTokenReference { get; }
    }
}