using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Exceptions;
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
        private ArrayList _references;

        /// <summary>
        /// Add Signed References to the Builder
        /// </summary>
        /// <param name="references"></param>
        /// <returns></returns>
        public NonRepudiationInformationBuilder WithSignedReferences(ArrayList references)
        {
            this._references = references;
            return this;
        }

        /// <summary>
        /// Create the <see cref="NonRepudiationInformation"/> Model
        /// </summary>
        /// <returns></returns>
        public NonRepudiationInformation Build()
        {
            PreConditionsBuilder();

            var nrrInformation = new NonRepudiationInformation();
            foreach (CryptoReference reference in this._references)
                AddMessagePartNRInformation(nrrInformation, reference);

            return nrrInformation;
        }

        private static void AddMessagePartNRInformation(NonRepudiationInformation nrrInformation, CryptoReference reference)
        {
            var partInfo = new MessagePartNRInformation
            {
                Reference = CreateReferenceFromCryptoRef(reference)
            };
            nrrInformation.MessagePartNRInformation.Add(partInfo);
        }

        private void PreConditionsBuilder()
        {
            if (this._references != null) return;
            const string description = "Builder needs signed references to create NonRepudiationInformation models";
            throw new AS4Exception(description);
        }

        private static CoreReference CreateReferenceFromCryptoRef(CryptoReference reference)
        {
            return new CoreReference
            {
                DigestMethod = new ReferenceDigestMethod(reference.DigestMethod),
                DigestValue = reference.DigestValue,
                URI = reference.Uri,
                Transforms = CreateTransformsFromChain(reference.TransformChain)
            };
        }

        private static Collection<ReferenceTransform> CreateTransformsFromChain(TransformChain transformChain)
        {
            var transforms = new Collection<ReferenceTransform>();
            foreach (Transform transform in transformChain)
            {
                transforms.Add(new ReferenceTransform(transform.Algorithm));
            }

            return transforms;
        }
    }
}