using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.IntegrationTests.Controllers;

namespace Eu.EDelivery.AS4.PayloadService.IntegrationTests
{
    class Program
    {
        public static void Main(string[] args)
        {
            Task task = Task.Run(() => new GivenPayloadControllerFacts().DownloadTheUploadedFileFromController());

            while (!task.IsCompleted) { }
        }
    }
}
