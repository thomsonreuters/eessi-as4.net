using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Controllers;
using Eu.EDelivery.AS4.PayloadService.Models;
using Eu.EDelivery.AS4.PayloadService.UnitTests.Models;
using Eu.EDelivery.AS4.PayloadService.UnitTests.Persistance;
using Eu.EDelivery.AS4.PayloadService.UnitTests.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Controllers
{
    public class GivenPayloadControllerFacts
    {
        private const string ExpectedContent = "message data!";
        private const string ExpectedHost = "localhost:4000";
        private const string ExpectedPath = "/api/Payload/";
        private const string ExpectedScheme = "http";

        private string ExpectedRequestUri => $"{ExpectedScheme}://{ExpectedHost}{ExpectedPath}.*";

        private PayloadController AnonymousPayloadController
            => new PayloadController(new CurrentDirectoryHostingEnvironment()) {ControllerContext = {HttpContext = new DefaultHttpContext()}};

        [Fact]
        public async Task DowmloadPayloadResultInNotFound_IfPayloadDoesntExists()
        {
            // Act
            IActionResult actualResult = await AnonymousPayloadController.Download("unknown-payload-id");

            // Assert
            Assert.IsType<NotFoundResult>(actualResult);
        }

        [Fact]
        public async Task UploadPayloadResultInBadRequest_IfContentTypeIsntMultipart()
        {
            // Act
            IActionResult actualResult = await AnonymousPayloadController.Upload();

            // Assert
            Assert.IsType<BadRequestObjectResult>(actualResult);
        }

        [Fact]
        public async Task DownloadsTheUploadedFileFromController()
        {
            // Arrange
            using (var contentStream = new MemoryStream())
            {
                PayloadController controller = AnonymousPayloadController;
                await SerializeExpectedContentStream(contentStream, controller);
                AssignRequestUri(controller.ControllerContext.HttpContext.Request);

                // Act
                var actualResult = await controller.Upload() as ObjectResult;

                // Assert
                var actualUploadResult = actualResult?.Value as UploadResult;
                Assert.True(Regex.IsMatch(actualUploadResult?.PayloadId, ExpectedRequestUri), $"Actual Request Uri doesn't match the expected Uri '{ExpectedRequestUri}'");

                StreamedFileResult downloadResult = await DownloadPayload(controller, actualUploadResult);
                StreamedFileResultAssert.OnContent(
                    downloadResult, actualContent => Assert.Equal(ExpectedContent, actualContent));
            }
        }

        private static void AssignRequestUri(HttpRequest request)
        {
            request.Host = new HostString(ExpectedHost);
            request.Path = $"{ExpectedPath}Upload";
            request.Scheme = ExpectedScheme;
        }

        private static async Task SerializeExpectedContentStream(Stream contentStream, ControllerBase controller)
        {
            var content = new MultipartFormDataContent
            {
                {new StreamContent(ExpectedContent.AsStream()), "filename", "filename"}
            };
            controller.ControllerContext.HttpContext.Request.ContentType = content.Headers.ContentType.ToString();

            await content.CopyToAsync(contentStream);
            contentStream.Position = 0;
            controller.ControllerContext.HttpContext.Request.Body = contentStream;
        }

        private static async Task<StreamedFileResult> DownloadPayload(PayloadController controller, UploadResult actualResult)
        {
            string payloadId = actualResult.PayloadId.Split('/').Last();

            return await controller.Download(payloadId) as StreamedFileResult;
        }
    }
}