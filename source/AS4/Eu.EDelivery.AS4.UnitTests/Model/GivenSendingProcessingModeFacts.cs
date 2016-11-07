using System;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing the <see cref="SendingProcessingMode"/>
    /// </summary>
    public class GivenSendingProcessingModeFacts
    {
        private SendingProcessingMode _pmode;

        /// <summary>
        /// Testing the PMode Model for its defaults
        /// </summary>
        public class GivenDefaultSendingProcessingMode : GivenSendingProcessingModeFacts
        {
            [Fact]
            public void ThenOverrideIsFalse()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.False(this._pmode.AllowOverride);
            }

            [Fact]
            public void ThenEncryptionIsFalse()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.Security);
                Assert.NotNull(this._pmode.Security.Encryption);
                Assert.False(this._pmode.Security.Encryption.IsEnabled);
            }

            [Fact]
            public void ThenSigningIsFalse()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.Security);
                Assert.NotNull(this._pmode.Security.Signing);
                Assert.False(this._pmode.Security.Signing.IsEnabled);
            }

            [Fact]
            public void ThenReliabilityIsNotNull()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.Reliability);
                Assert.NotNull(this._pmode.Reliability.ReceptionAwareness);
            }

            [Fact]
            public void ThenReceiptHandlingIsFalse()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.ReceiptHandling);
                Assert.NotNull(this._pmode.ReceiptHandling.NotifyMethod);
                Assert.False(this._pmode.ReceiptHandling.NotifyMessageProducer);
            }

            [Fact]
            public void ThenErrorHandlingIsFalse()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.ErrorHandling);
                Assert.NotNull(this._pmode.ErrorHandling.NotifyMethod);
                Assert.False(this._pmode.ErrorHandling.NotifyMessageProducer);
            }

            [Fact]
            public void ThenExceptionHandlingIsFalse()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.ExceptionHandling);
                Assert.NotNull(this._pmode.ExceptionHandling.NotifyMethod);
                Assert.False(this._pmode.ExceptionHandling.NotifyMessageProducer);
            }

            [Fact]
            public void ThenReceiptionAwerenessIsNotNull()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.ReceiptHandling);
                Assert.NotNull(this._pmode.ReceiptHandling.NotifyMethod);
            }

            [Fact]
            public void ThenReceiptionAwerenessIsDefault()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.False(this._pmode.Reliability.ReceptionAwareness.IsEnabled);
                Assert.Equal(5, this._pmode.Reliability.ReceptionAwareness.RetryCount);
                Assert.Equal("00:01:00", this._pmode.Reliability.ReceptionAwareness.RetryInterval);
            }

            [Fact]
            public void ThenPullConfigurationIsNotNull()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.PullConfiguration);
            }

            [Fact]
            public void ThenProtocolIsNotNull()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.PushConfiguration);
                Assert.NotNull(this._pmode.PushConfiguration.Protocol);
            }

            [Fact]
            public void ThenProtocolIsDefault()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.False(this._pmode.PushConfiguration.Protocol.UseChunking);
                Assert.False(this._pmode.PushConfiguration.Protocol.UseHttpCompression);
            }

            [Fact]
            public void ThenTlsConfigurationIsNotNull()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.NotNull(this._pmode.PushConfiguration.TlsConfiguration);
            }

            [Fact]
            public void ThenPushConfigurationIsDefault()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.False(this._pmode.PushConfiguration.TlsConfiguration.IsEnabled);
                Assert.Equal(TlsVersion.Tls12, this._pmode.PushConfiguration.TlsConfiguration.TlsVersion);
            }

            [Fact]
            public void ThenMessagePackagingIsDefault()
            {
                // Act
                this._pmode = new SendingProcessingMode();
                // Assert
                Assert.True(this._pmode.MessagePackaging.UseAS4Compression);
                Assert.False(this._pmode.MessagePackaging.IsMultiHop);
                Assert.False(this._pmode.MessagePackaging.IncludePModeId);
            }
        }
    }
}
