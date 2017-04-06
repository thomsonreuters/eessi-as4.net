using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Models
{
    public class StreamedFileResultAssert
    {
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