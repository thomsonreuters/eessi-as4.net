using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Models
{
    /// <summary>
    /// <see cref="StreamedFileResultAssert"/> assertion class.
    /// </summary>
    public class StreamedFileResultAssert
    {
        /// <summary>
        /// Assert on the given <paramref name="streamedFileResult"/> content with a custom <paramref name="assertion"/>.
        /// </summary>
        /// <param name="streamedFileResult">SUT (System under Test).</param>
        /// <param name="assertion">Custom assertion.</param>
        public static void OnContent(ActionResult streamedFileResult, Action<string> assertion)
        {
            using (var actualBody = new MemoryStream())
            {
                var actualContext = new ActionContext {HttpContext = new DefaultHttpContext {Response = {Body = actualBody}}};
                streamedFileResult?.ExecuteResult(actualContext);
                string actualContent = Encoding.UTF8.GetString(actualBody.ToArray());

                assertion(actualContent);
            }
        }
    }
}