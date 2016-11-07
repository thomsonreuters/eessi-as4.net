using System;
using System.IO;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Transforms;
using Eu.EDelivery.AS4.Xml;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Transforms
{
    /// <summary>
    /// Testing <see cref="AttachmentSignatureTransform" />
    /// </summary>
    public class GivenAttachmentSignatureTransformFacts
    {
        private AttachmentSignatureTransform _transform;

        public GivenAttachmentSignatureTransformFacts()
        {
            this._transform = new AttachmentSignatureTransform();
        }

        /// <summary>
        /// Testing if the transform succeeds
        /// </summary>
        public class GivenAttachmentSignatureTransformSucceeds
            : GivenAttachmentSignatureTransformFacts
        {
            [Theory]
            [InlineData("text/xml")]
            [InlineData("application/xml")]
            public void ThenOutputSucceedsWithXmlContentType(string contentType)
            {
                // Arrange
                var memoryStream = new MemoryStream();
                new XmlSerializer(typeof(PayloadInfo))
                    .Serialize(memoryStream, new PayloadInfo());
                memoryStream.Position = 0;

                base._transform = new AttachmentSignatureTransform(contentType);
                base._transform.LoadInput(memoryStream);

                // Act
                var output = base._transform.GetOutput(typeof(Stream)) as Stream;

                // Assert
                Assert.NotEqual(memoryStream, output);
            }

            [Fact]
            public void ThenAttachmentSignatureTransformReturnsExpectedUrl()
            {
                // Arrange
                this._transform = new AttachmentSignatureTransform();
                // Act
                string url = this._transform.Algorithm;
                // Assert
                const string attachmentSignature = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1" +
                                                   "#Attachment-Content-Signature-Transform";
                Assert.Equal(attachmentSignature, url);
            }

            [Fact]
            public void ThenOutputSucceedsWithSuportedType()
            {
                // Arrange
                MemoryStream memoryStream = LoadAttachmentTransform();
                Type type = typeof(Stream);
                // Act
                var output = base._transform.GetOutput(type) as MemoryStream;
                // Assert
                Assert.Equal(memoryStream, output);
            }

            private MemoryStream LoadAttachmentTransform()
            {
                var bytes = new byte[0];
                var memoryStream = new MemoryStream(bytes);
                base._transform = new AttachmentSignatureTransform();
                base._transform.LoadInput(memoryStream);
                return memoryStream;
            }
        }

        /// <summary>
        /// Testing if the transform fails
        /// </summary>
        public class GivenAttachmentSignatureTransformFails : GivenAttachmentSignatureTransformFacts
        {
            [Fact]
            public void ThenGetOutputFailsWithUnsuporrtedType()
            {
                // Arrange
                var @object = new object();
                // Act / Assert
                Assert.Throws<ArgumentException>(
                    () => base._transform.GetOutput(@object.GetType()));
            }
        }
    }
}