using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Model.Core;
using CoreReference = Eu.EDelivery.AS4.Model.Core.Reference;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Builders.Core
{
    /// <summary>
    /// Builder to create <see cref="NonRepudiationInformation"/> Models
    /// </summary>
    public class NonRepudiationInformationBuilder
    {
        private IEnumerable<CryptoReference> _references;

        /// <summary>
        /// Add Signed References to the Builder
        /// </summary>
        /// <param name="references"></param>
        /// <returns></returns>
        public NonRepudiationInformationBuilder WithSignedReferences(IEnumerable<CryptoReference> references)
        {
            _references = references;
            return this;
        }

        /// <summary>
        /// Create the <see cref="NonRepudiationInformation"/> Model
        /// </summary>
        /// <returns></returns>
        public NonRepudiationInformation Build()
        {
            if (_references == null || _references.Any(r => r is null))
            {
                throw new InvalidDataException(
                    "Builder needs signed references to create NonRepudiationInformation models");
            }

            return new NonRepudiationInformation(_references.Select(CreateReferenceFromCryptoRef));
        }

        private static CoreReference CreateReferenceFromCryptoRef(CryptoReference reference)
        {
            return new CoreReference(
                reference.Uri,
                CreateTransformsFromChain(reference.TransformChain),
                new ReferenceDigestMethod(reference.DigestMethod),
                reference.DigestValue);
        }

        private static IEnumerable<ReferenceTransform> CreateTransformsFromChain(TransformChain transformChain)
        {
            foreach (Transform transform in transformChain)
            {
                yield return new ReferenceTransform(transform.Algorithm);
            }
        }
    }
}