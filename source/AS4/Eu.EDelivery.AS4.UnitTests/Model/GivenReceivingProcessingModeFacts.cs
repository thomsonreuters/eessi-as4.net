using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing the <see cref="ReceivingProcessingMode" />
    /// </summary>
    public class GivenReceivingProcessingModeFacts
    {
        private ReceivingProcessingMode _pmode;

        /// <summary>
        /// Testing the PMode Model for its defaults
        /// </summary>
        public class GivenDefaultProcessingMode : GivenReceivingProcessingModeFacts
        {
            [Fact]
            public void ThenDecryptionIsDefault()
            {
                // Act
                _pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.Security);
                Assert.NotNull(_pmode.Security.Decryption);
                Assert.Equal(Limit.Allowed, _pmode.Security.Decryption.Encryption);
            }

            [Fact]
            public void ThenDeliverIsDefault()
            {
                // Act
                _pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.MessageHandling.DeliverInformation);
                Assert.False(_pmode.MessageHandling.DeliverInformation.IsEnabled);
            }

            [Fact]
            public void ThenDuplicateDetectionIsFalse()
            {
                // Act
                _pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.Reliability);
                Assert.NotNull(_pmode.Reliability.DuplicateElimination);
                Assert.False(_pmode.Reliability.DuplicateElimination.IsEnabled);
            }

            [Fact]
            public void ThenErrorHandlingIsDefault()
            {
                // Act
                _pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.ReplyHandling.ErrorHandling);
                Assert.False(_pmode.ReplyHandling.ErrorHandling.UseSoapFault);
            }

            [Fact]
            public void ThenExceptionHandlingIsDefault()
            {
                // Act
                _pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.ExceptionHandling);
                Assert.NotNull(_pmode.ExceptionHandling.NotifyMethod);
                Assert.False(_pmode.ExceptionHandling.NotifyMessageConsumer);
            }

            [Fact]
            public void ThenReceiptHandlingIsDefault()
            {
                // Act
                _pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.ReplyHandling.ReceiptHandling);
                Assert.False(_pmode.ReplyHandling.ReceiptHandling.UseNNRFormat);
                Assert.Equal(ReplyPattern.Response, _pmode.ReplyHandling.ReplyPattern);
            }

            [Fact]
            public void ThenSigningVerificationIsDefault()
            {
                // Act
                _pmode = new ReceivingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.Security);
                Assert.NotNull(_pmode.Security.SigningVerification);
                Assert.Equal(Limit.Allowed, _pmode.Security.SigningVerification.Signature);
            }
        }
    }
}