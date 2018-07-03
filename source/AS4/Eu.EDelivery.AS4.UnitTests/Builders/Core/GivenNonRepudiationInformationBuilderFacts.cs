using System;
using System.Linq;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Transforms;
using Xunit;
using Reference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Core
{
    /// <summary>
    /// Testing <see cref="NonRepudiationInformationBuilder" />
    /// </summary>
    public class GivenNonRepudiationInformationBuilderFacts
    {
        public class GivenValidArguments : GivenNonRepudiationInformationBuilderFacts
        {
            private static void AssertReferenceDigest(Reference reference, NonRepudiationInformation nonRepudiation)
            {
                AS4.Model.Core.Reference firstReference = nonRepudiation.MessagePartNRIReferences.First();
                Assert.True(reference.DigestValue.SequenceEqual(firstReference.DigestValue));
                var referenceDigestMethod = new ReferenceDigestMethod(reference.DigestMethod);
                Assert.Equal(referenceDigestMethod.Algorithm, firstReference.DigestMethod.Algorithm);
            }

            [Fact]
            public void ThenBuildNonRepudiationInformationSucceedsWithSignedReferencesForDigest()
            {
                // Arrange
                Reference reference = CreateDefaultSignedReference();
                var references = new[] { reference };

                // Act
                NonRepudiationInformation nonRepudiation =
                    new NonRepudiationInformationBuilder().WithSignedReferences(references).Build();

                // Assert
                AssertReferenceDigest(reference, nonRepudiation);
            }

            [Fact]
            public void ThenBuildNonRepudiationInformationSucceedsWithSignedReferencesForTransforms()
            {
                // Arrange
                Reference reference = CreateDefaultSignedReference();
                var references = new[] { reference };

                // Act
                NonRepudiationInformation nonRepudiation =
                    new NonRepudiationInformationBuilder().WithSignedReferences(references).Build();

                // Assert
                AS4.Model.Core.Reference firstReference = nonRepudiation.MessagePartNRIReferences.First();
                ReferenceTransform referenceTransform = firstReference.Transforms.First();
                Assert.Equal(reference.TransformChain[0].Algorithm, referenceTransform.Algorithm);
            }

            [Fact]
            public void ThenBuildNonRepudiationInformationSucceedsWithSignedReferencesForUri()
            {
                // Arrange
                Reference reference = CreateDefaultSignedReference();
                var references = new[] { reference };

                // Act
                NonRepudiationInformation nonRepudiation =
                    new NonRepudiationInformationBuilder().WithSignedReferences(references).Build();

                // Assert
                AS4.Model.Core.Reference firstReference = nonRepudiation.MessagePartNRIReferences.First();
                Assert.Equal(reference.Uri, firstReference.URI);
            }
        }

        public class GivenInvalidArguments : GivenNonRepudiationInformationBuilderFacts
        {
            [Fact]
            public void ThenBuildNonRepudiationInformationFailsWithMissingSignedReferences()
            {
                // Act / Assert
                Assert.ThrowsAny<Exception>(() => new NonRepudiationInformationBuilder().Build());
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

        private static TransformChain CreateDefaulTransformChain()
        {
            var transformChain = new TransformChain();
            transformChain.Add(new AttachmentSignatureTransform { Algorithm = Guid.NewGuid().ToString() });

            return transformChain;
        }
    }
}