using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.UnitTests.Controllers;
using Eu.EDelivery.AS4.PayloadService.UnitTests.Infrastructure;
using Eu.EDelivery.AS4.PayloadService.UnitTests.Models;
using Eu.EDelivery.AS4.PayloadService.UnitTests.Persistance;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            DeletePaylods();

            Task task = Task.Run(RunTests);

            while (!task.IsCompleted) {}

            WaitUntilKeyIsPressed();
        }

        private static void DeletePaylods()
        {
            foreach (string file in Directory.EnumerateFiles(Path.Combine("Payloads")))
            {
                File.Delete(file);
            }
        }

        private static async Task RunTests()
        {
            Console.WriteLine("Run Payload Integration Tests...");

            await RunTestAsync(new GivenPayloadControllerFacts().DownloadsTheUploadedFileFromController);
            await RunTestAsync(new GivenPayloadControllerFacts().DowmloadPayloadResultInNotFound_IfPayloadDoesntExists);
            await RunTestAsync(new GivenPayloadControllerFacts().UploadPayloadResultInBadRequest_IfContentTypeIsntMultipart);
            await RunTestAsync(new GivenFilePayloadPersisterFacts().WritesFileWithMetaToDisk);
            await RunTestAsync(new GivenFilePayloadPersisterFacts().LoadsPayloadWithMetaFromDisk);

            RunTest(new GivenMultipartPayloadReaderFacts().CannotCreateReader_IfContentTypeIsntMultiPart);
            await RunTestAsync(new GivenMultipartPayloadReaderFacts().ReadsExpectedContent);
            RunTest(new GivenStreamedFileResultFacts().ReturnsExpectedResult);
            RunTest(new GivenPayloadFacts().ThenPayloadNullObjectIsEqualToSelfCreatedObject);
        }

        private static async Task RunTestAsync(Func<Task> testRun)
        {
            try
            {
                await testRun();
                LogTestRun(testRun.GetMethodInfo());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void RunTest(Action testRun)
        {
            try
            {
                testRun();
                LogTestRun(testRun.GetMethodInfo());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void LogTestRun(MemberInfo methodInfo)
        {
            Console.WriteLine($"Passed: {methodInfo.DeclaringType.Name}+{methodInfo.Name}");
        }

        private static void WaitUntilKeyIsPressed()
        {
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
            } while (key.Key != ConsoleKey.Q);
        }
    }
}