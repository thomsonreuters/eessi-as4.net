using Eu.EDelivery.AS4.Fe.Monitor;
using Eu.EDelivery.AS4.Fe.Monitor.Model;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class ExceptionMessageTests
    {
        private string exception;
        private ExceptionMessage exceptionMessage;

        private void Setup()
        {
            exception = @"[9acd3265-cd3a-4903-9ec4-694fc4433c34@mindertestbed.org]Decryption failed
   at Eu.EDelivery.AS4.Steps.Receive.DecryptAS4MessageStep.TryDecryptAS4Message() in C:\Dev\codit.visualstudio.com\AS4.NET\source\Steps\Eu.EDelivery.AS4.Steps\Receive\DecryptAS4MessageStep.cs:line 109
   at Eu.EDelivery.AS4.Steps.Receive.DecryptAS4MessageStep.ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken) in C:\Dev\codit.visualstudio.com\AS4.NET\source\Steps\Eu.EDelivery.AS4.Steps\Receive\DecryptAS4MessageStep.cs:line 66
   at Eu.EDelivery.AS4.Steps.CompositeStep.<ExecuteAsync>d__2.MoveNext() in C:\Dev\codit.visualstudio.com\AS4.NET\source\AS4\Eu.EDelivery.AS4\Steps\CompositeStep.cs:line 43
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at Eu.EDelivery.AS4.Steps.Receive.ReceiveExceptionStepDecorator.<ExecuteAsync>d__4.MoveNext() in C:\Dev\codit.visualstudio.com\AS4.NET\source\Steps\Eu.EDelivery.AS4.Steps\Receive\ReceiveExceptionStepDecorator.cs:line 54
Failed to decrypt data element
   at Eu.EDelivery.AS4.Security.Strategies.EncryptionStrategy.TryDecryptEncryptedData(EncryptedData encryptedData) in C:\Dev\codit.visualstudio.com\AS4.NET\source\AS4\Eu.EDelivery.AS4\Security\Strategies\EncryptionStrategy.cs:line 288
   at Eu.EDelivery.AS4.Security.Strategies.EncryptionStrategy.DecryptMessage() in C:\Dev\codit.visualstudio.com\AS4.NET\source\AS4\Eu.EDelivery.AS4\Security\Strategies\EncryptionStrategy.cs:line 271
   at Eu.EDelivery.AS4.Model.Core.SecurityHeader.Decrypt(IEncryptionStrategy encryptionStrategy) in C:\Dev\codit.visualstudio.com\AS4.NET\source\AS4\Eu.EDelivery.AS4\Model\Core\SecurityHeader.cs:line 124
   at Eu.EDelivery.AS4.Steps.Receive.DecryptAS4MessageStep.TryDecryptAS4Message() in C:\Dev\codit.visualstudio.com\AS4.NET\source\Steps\Eu.EDelivery.AS4.Steps\Receive\DecryptAS4MessageStep.cs:line 104
";

            exceptionMessage = new ExceptionMessage
            {
                Exception = exception
            };
        }

        [Fact]
        public void When_Setting_Exception_Then_Exception_Short_Should_Be_Updated()
        {
            Setup();

            Assert.True(!string.IsNullOrEmpty(exceptionMessage.ExceptionShort));
        }

        [Fact]
        public void ExceptionShort_Must_Only_Contain_The_First_Line_Without_Message_Id()
        {
            Setup();

            Assert.True(exceptionMessage.ExceptionShort == "Decryption failed");
        }

        [Fact]
        public void No_Exception_Should_Be_Thrown_When_The_Exception_Doesnt_Contain_The_MessageId()
        {
            Setup();
            exceptionMessage.Exception = "Decryption failed";

            Assert.True(exceptionMessage.ExceptionShort == "Decryption failed");
        }

        [Fact]
        public void No_Exception_Should_Be_Thrown_When_Exception_Is_Empty()
        {
            Setup();
            exceptionMessage.Exception = string.Empty;

            Assert.True(string.IsNullOrEmpty(exceptionMessage.Exception));
            Assert.True(string.IsNullOrEmpty(exceptionMessage.ExceptionShort));
        }
    }
}