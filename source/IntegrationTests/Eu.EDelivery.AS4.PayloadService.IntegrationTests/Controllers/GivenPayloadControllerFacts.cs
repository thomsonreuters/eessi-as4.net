using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Controllers;
using Eu.EDelivery.AS4.PayloadService.Infrastructure;
using Eu.EDelivery.AS4.PayloadService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Eu.EDelivery.AS4.PayloadService.IntegrationTests.Controllers
{
    public class GivenPayloadControllerFacts
    {
        private const string ExpectedContent = "message data!";

        [Fact]
        public async Task DownloadTheUploadedFileFromController()
        {
            // Arrange
            using (var contentStream = new MemoryStream())
            {
                var controller = new PayloadController(new StubHostingEnvironment()) {ControllerContext = {HttpContext = new DefaultHttpContext()}};
                await SerializeExpectedContentStream(contentStream, controller);

                // Act
                var actualResult = await controller.Upload() as ObjectResult;

                // Assert
                StreamedFileResult downloadResult = DownloadPayload(controller, actualResult);
                AssertDownloadWithUpload(downloadResult);
            }
        }

        private static async Task SerializeExpectedContentStream(Stream contentStream, ControllerBase controller)
        {
            var content = new MultipartFormDataContent
            {
                {new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(ExpectedContent))), "filename", "filename"}
            };
            controller.ControllerContext.HttpContext.Request.ContentType = content.Headers.ContentType.ToString();

            await content.CopyToAsync(contentStream);
            contentStream.Position = 0;
            controller.ControllerContext.HttpContext.Request.Body = contentStream;
        }

        private static StreamedFileResult DownloadPayload(PayloadController controller, ObjectResult actualResult)
        {
            var uploadResult = actualResult?.Value as UploadResult;
            var downloadResult = controller.Download(uploadResult?.PayloadId) as StreamedFileResult;
            return downloadResult;
        }

        private static void AssertDownloadWithUpload(ActionResult downloadResult)
        {
            using (var actualBody = new MemoryStream())
            {
                var actualContext = new ActionContext { HttpContext = new DefaultHttpContext { Response = { Body = actualBody } } };
                downloadResult?.ExecuteResult(actualContext);
                string actualContent = Encoding.UTF8.GetString(actualBody.ToArray());

                Assert.Equal(ExpectedContent, actualContent);
            }
        }
    }
}