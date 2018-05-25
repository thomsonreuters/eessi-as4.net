using System;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing the <see cref="ReceivingProcessingMode" />
    /// </summary>
    public class GivenReceivingProcessingModeFacts
    {        
        /// <summary>
        /// Testing the PMode Model for its defaults
        /// </summary>
        public class GivenDefaultProcessingMode : GivenReceivingProcessingModeFacts
        {
            [Fact]
            public void ThenDecryptionIsIgnoredByDefault()
            {
                // Act
                var pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(pmode.Security);
                Assert.NotNull(pmode.Security.Decryption);
                Assert.Equal(Limit.Ignored, pmode.Security.Decryption.Encryption);
            }

            [Fact]
            public void ThenDeliverIsDefault()
            {
                // Act
                var pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(pmode.MessageHandling.DeliverInformation);
                Assert.False(pmode.MessageHandling.DeliverInformation.IsEnabled);
            }

            [Fact]
            public void ThenRetryReliabilityHasDefaultTimeSpan()
            {
                // Act
                var pmode = new ReceivingProcessingMode();
                pmode.MessageHandling.DeliverInformation.Reliability.RetryIntervalString = Guid.NewGuid().ToString();

                // Assert
                Assert.Equal(default(TimeSpan), pmode.MessageHandling.DeliverInformation.Reliability.RetryInterval);
            }

            [Fact]
            public void ThenDuplicateDetectionIsFalse()
            {
                // Act
                var pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(pmode.Reliability);
                Assert.NotNull(pmode.Reliability.DuplicateElimination);
                Assert.False(pmode.Reliability.DuplicateElimination.IsEnabled);
            }

            [Fact]
            public void ThenErrorHandlingIsDefault()
            {
                // Act
                var pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(pmode.ReplyHandling.ErrorHandling);
                Assert.False(pmode.ReplyHandling.ErrorHandling.UseSoapFault);
            }

            [Fact]
            public void ThenExceptionHandlingIsDefault()
            {
                // Act
                var pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(pmode.ExceptionHandling);
                Assert.NotNull(pmode.ExceptionHandling.NotifyMethod);
                Assert.False(pmode.ExceptionHandling.NotifyMessageConsumer);
            }

            [Fact]
            public void ThenReceiptHandlingIsDefault()
            {
                // Act
                var pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(pmode.ReplyHandling.ReceiptHandling);
                Assert.False(pmode.ReplyHandling.ReceiptHandling.UseNRRFormat);
                Assert.Equal(ReplyPattern.Response, pmode.ReplyHandling.ReplyPattern);
            }

            [Fact]
            public void ThenSigningVerificationIsDefault()
            {
                // Act
                var pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(pmode.Security);
                Assert.NotNull(pmode.Security.SigningVerification);
                Assert.Equal(Limit.Allowed, pmode.Security.SigningVerification.Signature);
            }
        }
    }
}