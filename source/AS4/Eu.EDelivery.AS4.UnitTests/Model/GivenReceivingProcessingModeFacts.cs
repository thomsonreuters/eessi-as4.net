using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing the <see cref="ReceivingProcessingMode"/>
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
            public void ThenDuplicateDetectionIsFalse()
            {
                // Act
                this._pmode = new ReceivingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.Reliability);
                Assert.NotNull(this._pmode.Reliability.DuplicateElimination);
                Assert.False(this._pmode.Reliability.DuplicateElimination.IsEnabled);
            }

            [Fact]
            public void ThenReceiptHandlingIsDefault()
            {
                // Act
                this._pmode = new ReceivingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.ReceiptHandling);
                Assert.False(this._pmode.ReceiptHandling.UseNNRFormat);
                Assert.Equal(ReplyPattern.Response, this._pmode.ReceiptHandling.ReplyPattern);
            }

            [Fact]
            public void ThenErrorHandlingIsDefault()
            {
                // Act
                this._pmode = new ReceivingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.ErrorHandling);
                Assert.False(this._pmode.ErrorHandling.UseSoapFault);
            }

            [Fact]
            public void ThenExceptionHandlingIsDefault()
            {
                // Act
                this._pmode = new ReceivingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.ExceptionHandling);
                Assert.NotNull(this._pmode.ExceptionHandling.NotifyMethod);
                Assert.False(this._pmode.ExceptionHandling.NotifyMessageConsumer);
            }

            [Fact]
            public void ThenSigningVerificationIsDefault()
            {
                // Act
                this._pmode = new ReceivingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.Security);
                Assert.NotNull(this._pmode.Security.SigningVerification);
                Assert.Equal(Limit.Allowed, this._pmode.Security.SigningVerification.Signature);
            }

            [Fact]
            public void ThenDecryptionIsDefault()
            {
                // Act
                this._pmode = new ReceivingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.Security);
                Assert.NotNull(this._pmode.Security.Decryption);
                Assert.Equal(Limit.Allowed, this._pmode.Security.Decryption.Encryption);
            }

            [Fact]
            public void ThenDeliverIsDefault()
            {
                // Act
                this._pmode = new ReceivingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.Deliver);
                Assert.False(this._pmode.Deliver.IsEnabled);
            }
        }
    }
}
