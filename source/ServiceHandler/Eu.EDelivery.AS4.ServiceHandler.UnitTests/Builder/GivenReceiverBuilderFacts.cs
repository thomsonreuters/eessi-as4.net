using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.ServiceHandler.Builder;
using Xunit;

namespace Eu.EDelivery.AS4.ServiceHandler.UnitTests.Builder
{
    /// <summary>
    /// Testing <see cref="ReceiverBuilder"/>
    /// </summary>
    public class GivenReceiverBuilderFacts
    {
        public class GivenValidArguments : GivenReceiverBuilderFacts
        {
            [Fact]
            public void ThenBuilderCreatesValidReceiver()
            {
                // Arrange
                Receiver settingReceiver = CreateDefaultReceiverSettings();

                // Act
                IReceiver receiver = new ReceiverBuilder().SetSettings(settingReceiver).Build();

                // Assert
                Assert.NotNull(receiver);
                Assert.IsType<FileReceiver>(receiver);
            }

            private Receiver CreateDefaultReceiverSettings()
            {
                return new Receiver
                {
                    Type = typeof(FileReceiver).AssemblyQualifiedName,
                    Setting = new[] { new Setting { Key = "Test", Value = "Test" } }
                };
            }
        }
    }
}