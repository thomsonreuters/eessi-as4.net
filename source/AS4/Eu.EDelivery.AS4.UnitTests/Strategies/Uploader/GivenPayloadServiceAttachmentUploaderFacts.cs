using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Eu.EDelivery.AS4.UnitTests.Strategies.Method;
using Newtonsoft.Json;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Uploader
{
    public class GivenPayloadServiceAttachmentUploaderFacts
    {
        [Fact]
        public async Task ThenUploadAttachmentSucceeds()
        {
            // Arrange
            UploadResult expectedResult = CreateAnonymousUploadResult();
            var uploader = new PayloadServiceAttachmentUploader((uri, content) => PostRequest(expectedResult));
            uploader.Configure(new LocationMethod(null));

            // Act
            UploadResult actualResult = await uploader.UploadAsync(CreateAnonymousAttachment(), new UserMessage());

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        private static Task<HttpResponseMessage> PostRequest(UploadResult expectedResult)
        {
            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(expectedResult))
            };

            return Task.FromResult(response);
        }

        [Fact]
        public async Task ThenUploadAttachmentFails_IfPayloadServiceIsNotRunning()
        {
            var uploader = new PayloadServiceAttachmentUploader();
            uploader.Configure(new LocationMethod(null));

            await Assert.ThrowsAnyAsync<Exception>(() => uploader.UploadAsync(CreateAnonymousAttachment(), new UserMessage()));
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
