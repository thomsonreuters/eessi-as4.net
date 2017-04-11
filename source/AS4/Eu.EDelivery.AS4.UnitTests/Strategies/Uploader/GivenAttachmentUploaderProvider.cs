using System.Collections;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Uploader
{
    /// <summary>
    /// Testing <see cref="GivenAttachmentUploaderProvider" />
    /// </summary>
    public class GivenAttachmentUploaderProvider
    {
        [Theory]
        [ClassData(typeof(AttachmentUploaderSource))]
        public void AttachmentProviderGetsUploader_IfUploaderGetsAccepted(string expectedKey, IAttachmentUploader expectedUploader)
        {
            // Arrange
            var provider = new AttachmentUploaderProvider();
            provider.Accept(s => s.Equals(expectedKey), expectedUploader);

            // Act
            IAttachmentUploader actualUploader = provider.Get(expectedKey);

            // Assert
            Assert.Equal(expectedUploader, actualUploader);
        }

        private class AttachmentUploaderSource : IEnumerable<object[]>
        {
            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {"FILE", new FileAttachmentUploader(null)};
                yield return new object[] {"EMAIL", new EmailAttachmentUploader(null)};
                yield return new object[] {"PAYLOAD-SERVICE", new PayloadServiceAttachmentUploader()};
            }
        }
    }
}