using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Transforms;
using Xunit;
using Reference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Core
{
    /// <summary>
    /// Testing <see cref="NonRepudiationInformationBuilder"/>
    /// </summary>
    public class GivenNonRepudiationInformationBuilderFacts
    {
        public class GivenValidArguments : GivenNonRepudiationInformationBuilderFacts
        {
            [Fact]
            public void ThenBuildNonRepudiationInformationSucceedsWithSignedReferencesForUri()
            {
                // Arrange
                Reference reference = base.CreateDefaultSignedReference();
                var references = new ArrayList { reference };
                // Act
                NonRepudiationInformation nonRepudiation = new NonRepudiationInformationBuilder()
                    .WithSignedReferences(references).Build();
                // Assert
                MessagePartNRInformation partNRInformation = nonRepudiation.MessagePartNRInformation.First();
                Assert.Equal(reference.Uri, partNRInformation.Reference.URI);
            }

            [Fact]
            public void ThenBuildNonRepudiationInformationSucceedsWithSignedReferencesForTransforms()
            {
                // Arrange
                Reference reference = base.CreateDefaultSignedReference();
                var references = new ArrayList { reference };
                // Act
                NonRepudiationInformation nonRepudiation = new NonRepudiationInformationBuilder()
                    .WithSignedReferences(references).Build();
                // Assert
                MessagePartNRInformation partNRInformation = nonRepudiation.MessagePartNRInformation.First();
                ReferenceTransform referenceTransform = partNRInformation.Reference.Transforms.First();
                Assert.Equal(reference.TransformChain[0].Algorithm, referenceTransform.Algorithm);
            }

            [Fact]
            public void ThenBuildNonRepudiationInformationSucceedsWithSignedReferencesForDigest()
            {
                // Arrange
                Reference reference = base.CreateDefaultSignedReference();
                var references = new ArrayList { reference };
                // Act
                NonRepudiationInformation nonRepudiation = new NonRepudiationInformationBuilder()
                    .WithSignedReferences(references).Build();
                // Assert
                AssertReferenceDigest(reference, nonRepudiation);
            }

            private static void AssertReferenceDigest(Reference reference, NonRepudiationInformation nonRepudiation)
            {
                MessagePartNRInformation partNRInformation = nonRepudiation.MessagePartNRInformation.First();
                Assert.True(Enumerable.SequenceEqual(reference.DigestValue, partNRInformation.Reference.DigestValue));
                var referenceDigestMethod = new ReferenceDigestMethod(reference.DigestMethod);
                Assert.Equal(referenceDigestMethod.Algorithm, partNRInformation.Reference.DigestMethod.Algorithm);
            }
        }

        public class GivenInvalidArguments : GivenNonRepudiationInformationBuilderFacts
        {
            [Fact]
            public void ThenBuildNonRepudiationInformationFailsWithMissingSignedReferences()
            {
                // Act / Assert
                Assert.Throws<AS4Exception>(()
                    => new NonRepudiationInformationBuilder().Build());
            }
        }

        protected Reference CreateDefaultSignedReference()
        {
            return new Reference
            {
                DigestValue = new byte[0],
                DigestMethod = Guid.NewGuid().ToString(),
                TransformChain = CreateDefaulTransformChain(),
                Uri = Guid.NewGuid().ToString()
            };
        }

        private TransformChain CreateDefaulTransformChain()
        {
            var transformChain = new TransformChain();
            transformChain.Add(new AttachmentSignatureTransform
            {
                Algorithm = Guid.NewGuid().ToString()
            });
            return transformChain;
        }
    }
}