using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Eu.EDelivery.AS4.UnitTests.Http;
using Eu.EDelivery.AS4.UnitTests.Strategies.Method;
using Newtonsoft.Json;
using SimpleHttpMock;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Uploader
{
    public class GivenPayloadServiceAttachmentUploaderFacts
    {
        private static readonly string SharedUrl = UniqueHost.Create();

        [Fact]
        public async Task ThenUploadAttachmentSucceeds()
        {
            // Arrange
            var uploader = new PayloadServiceAttachmentUploader();
            uploader.Configure(new LocationMethod(SharedUrl));
            UploadResult expectedResult = CreateAnonymousUploadResult();

            using (UseStubbedHttpServerThatReturns(expectedResult))
            {
                // Act
                UploadResult actualResult = await uploader.Upload(CreateAnonymousAttachment());

                // Assert
                Assert.Equal(expectedResult, actualResult);
            }
        }

        private static IDisposable UseStubbedHttpServerThatReturns(UploadResult expectedResult)
        {
            var builder = new MockedHttpServerBuilder();

            builder.WhenPost(SharedUrl).RespondContent(
                HttpStatusCode.OK,
                request =>
                {
                    string serializedContent = JsonConvert.SerializeObject(expectedResult);
                    return new StringContent(serializedContent);
                });

            return builder.Build(SharedUrl);
        }

        [Fact]
        public async Task ThenUploadAttachmentFails_IfPayloadServiceIsNotRunning()
        {
            var uploader = new PayloadServiceAttachmentUploader();
            uploader.Configure(new LocationMethod(SharedUrl));

            await Assert.ThrowsAsync<AS4Exception>(() => uploader.Upload(CreateAnonymousAttachment()));
        }

        private static UploadResult CreateAnonymousUploadResult()
        {
            return new UploadResult {DownloadUrl = "ignored download url", PayloadId = "ignored payload id"};
        }

        private static Attachment CreateAnonymousAttachment()
        {
            return new Attachment {Content = Stream.Null};
        }
    }
}
