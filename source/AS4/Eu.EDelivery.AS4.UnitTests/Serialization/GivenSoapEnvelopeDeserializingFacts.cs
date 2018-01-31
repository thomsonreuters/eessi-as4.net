using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    public class GivenSoapEnvelopeDeserializingFacts
    {
        public class DeserializingSucceedsFacts
        {
            [Fact]
            public async Task CanDeserializeEncryptedMessage()
            {
                var message = await DeserializeAS4Message(Properties.Resources.as4_encrypted_message,
                                                          @"multipart/related; boundary=""MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11""");

                try
                {
                    Assert.NotNull(message);
                    Assert.NotNull(message.EnvelopeDocument);
                    Assert.NotNull(message.SecurityHeader);
                    Assert.True(message.SecurityHeader.IsEncrypted);

                    Assert.True(message.IsUserMessage);
                    Assert.Equal("30392f3c-bc9c-4a1f-ba5e-5d4d81382eef@CLT-SNEIRINCK", message.PrimaryUserMessage.MessageId);
                }
                finally
                {
                    message.CloseAttachments();
                }
            }

            [Fact]
            public async Task CanDeserializeSignedMultihopErrorMessage()
            {
                var message = await DeserializeAS4Message(Properties.Resources.as4_multihop_message,
                                                          "multipart/related; boundary=\"=-M/sMGEhQK8RBNg/21Nf7Ig==\";\ttype=\"application/soap+xml\"");

                try
                {
                    Assert.NotNull(message);
                    Assert.NotNull(message.EnvelopeDocument);
                    Assert.NotNull(message.SecurityHeader);
                    Assert.True(message.SecurityHeader.IsSigned);
                    Assert.True(message.IsSignalMessage);
                    Assert.True(message.IsMultiHopMessage);

                    Assert.Equal("1e022294-1809-4c9e-b98d-27ef93be5f13@TESTFX", message.GetPrimaryMessageId());
                }
                finally
                {
                    message.CloseAttachments();
                }
            }

            [Fact]
            public async Task CanDeserializeSignedAndEncryptedFlameEnvelope()
            {
                var message = await DeserializeAS4Message(Properties.Resources.as4_flame_envelope, "application/soap+xml");

                try
                {
                    Assert.NotNull(message);
                    Assert.NotNull(message.EnvelopeDocument);
                    Assert.NotNull(message.SecurityHeader);
                    Assert.True(message.SecurityHeader.IsEncrypted);
                    Assert.True(message.SecurityHeader.IsSigned);

                    Assert.True(message.IsUserMessage);
                    Assert.Equal("b495f1e1-ef0f-4da3-a311-0f5ab6ab27a9@mindertestbed.org", message.PrimaryUserMessage.MessageId);
                }
                finally
                {
                    message.CloseAttachments();
                }
            }

            [Fact]
            public async Task CanDeserializeSignedHolodeckMessage()
            {
                var message = await DeserializeAS4Message(Properties.Resources.signed_holodeck_message,
                                                          @"multipart/related;boundary=""MIMEBoundary_bcb27a6f984295aa9962b01ef2fb3e8d982de76d061ab23f""");
                try
                {

                    Assert.NotNull(message);
                    Assert.NotNull(message.EnvelopeDocument);
                    Assert.NotNull(message.SecurityHeader);
                    Assert.False(message.SecurityHeader.IsEncrypted);
                    Assert.True(message.SecurityHeader.IsSigned);

                    Assert.True(message.IsUserMessage);
                    Assert.Equal("cdcc838c-96ed-414a-b127-0937f5cd6549@CLT-FGHEYSELS.ad.codit.eu", message.PrimaryUserMessage.MessageId);
                }
                finally
                {
                    message.CloseAttachments();
                }
            }

            private static async Task<AS4Message> DeserializeAS4Message(byte[] message, string contentType)
            {
                var stream = new MemoryStream(message);

                var serializer = SerializerProvider.Default.Get(contentType);

                return await serializer.DeserializeAsync(stream, contentType, CancellationToken.None);
            }
        }
    }
}
