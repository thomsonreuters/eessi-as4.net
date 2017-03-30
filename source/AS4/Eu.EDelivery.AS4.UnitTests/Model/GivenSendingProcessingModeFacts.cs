using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing the <see cref="SendingProcessingMode" />
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
            public void ThenEncryptionIsFalse()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.Security);
                Assert.NotNull(_pmode.Security.Encryption);
                Assert.False(_pmode.Security.Encryption.IsEnabled);
            }

            [Fact]
            public void ThenErrorHandlingIsFalse()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.ErrorHandling);
                Assert.NotNull(_pmode.ErrorHandling.NotifyMethod);
                Assert.False(_pmode.ErrorHandling.NotifyMessageProducer);
            }

            [Fact]
            public void ThenExceptionHandlingIsFalse()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.ExceptionHandling);
                Assert.NotNull(_pmode.ExceptionHandling.NotifyMethod);
                Assert.False(_pmode.ExceptionHandling.NotifyMessageProducer);
            }

            [Fact]
            public void ThenMessagePackagingIsDefault()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.True(_pmode.MessagePackaging.UseAS4Compression);
                Assert.False(_pmode.MessagePackaging.IsMultiHop);
                Assert.False(_pmode.MessagePackaging.IncludePModeId);
            }

            [Fact]
            public void ThenOverrideIsFalse()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.False(_pmode.AllowOverride);
            }

            [Fact]
            public void ThenProtocolIsDefault()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.False(_pmode.PushConfiguration.Protocol.UseChunking);
                Assert.False(_pmode.PushConfiguration.Protocol.UseHttpCompression);
            }

            [Fact]
            public void ThenProtocolIsNotNull()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.PushConfiguration);
                Assert.NotNull(_pmode.PushConfiguration.Protocol);
            }

            [Fact]
            public void ThenPullConfigurationIsNotNull()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.PullConfiguration);
            }

            [Fact]
            public void ThenPushConfigurationIsDefault()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.False(_pmode.PushConfiguration.TlsConfiguration.IsEnabled);
                Assert.Equal(TlsVersion.Tls12, _pmode.PushConfiguration.TlsConfiguration.TlsVersion);
            }

            [Fact]
            public void ThenReceiptHandlingIsFalse()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.ReceiptHandling);
                Assert.NotNull(_pmode.ReceiptHandling.NotifyMethod);
                Assert.False(_pmode.ReceiptHandling.NotifyMessageProducer);
            }

            [Fact]
            public void ThenReceiptionAwerenessIsDefault()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.False(_pmode.Reliability.ReceptionAwareness.IsEnabled);
                Assert.Equal(5, _pmode.Reliability.ReceptionAwareness.RetryCount);
                Assert.Equal("00:01:00", _pmode.Reliability.ReceptionAwareness.RetryInterval);
            }

            [Fact]
            public void ThenReceiptionAwerenessIsNotNull()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.ReceiptHandling);
                Assert.NotNull(_pmode.ReceiptHandling.NotifyMethod);
            }

            [Fact]
            public void ThenReliabilityIsNotNull()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.Reliability);
                Assert.NotNull(_pmode.Reliability.ReceptionAwareness);
            }

            [Fact]
            public void ThenSigningIsFalse()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.Security);
                Assert.NotNull(_pmode.Security.Signing);
                Assert.False(_pmode.Security.Signing.IsEnabled);
            }

            [Fact]
            public void ThenTlsConfigurationIsNotNull()
            {
                // Act
                _pmode = new SendingProcessingMode();

                // Assert
                Assert.NotNull(_pmode.PushConfiguration.TlsConfiguration);
            }
        }
    }
}