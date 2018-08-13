using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ServiceHandler;
using Eu.EDelivery.AS4.UnitTests.Agents;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Servicehandler
{
    /// <summary>
    /// Testing <see cref="Kernel"/>
    /// </summary>
    public class GivenKernelFacts
    {
        [Fact]
        public void KernelHasNothingToClose()
        {
            // Arrange
            var spyAgent = new SpyAgent();
            var sut = new Kernel(agents: null);

            // Act 
            sut.Dispose();

            // Assert
            Assert.False(spyAgent.IsDisposed);
        }

        [Fact]
        public async Task KernelHasNothingToStart()
        {
            // Arrange
            var spyAgent = new SpyAgent();
            var sut = new Kernel(agents: null);

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            Assert.False(spyAgent.HasStarted);
        }

        [Fact]
        public void DisposeAgents_FromKernel()
        {
            // Arrange
            var spyAgent = new SpyAgent();
            var kernel = new Kernel(new[] { spyAgent });

            // Act
            kernel.Dispose();

            // Assert
            Assert.True(spyAgent.IsDisposed);
        }

        [Fact]
        public async Task StartAgents_FromKernel()
        {
            // Arrange
            var spyAgent = new SpyAgent();
            var kernel = new Kernel(new[] { spyAgent }, StubConfig.Default);
            var cancellationSource = new CancellationTokenSource();

            // Act
            await kernel.StartAsync(cancellationSource.Token);

            // Assert
            Assert.True(spyAgent.HasStarted);

            // TearDown
            cancellationSource.Cancel();
        }

        [Fact]
        public async Task FailToStartAgents_IfInvalidConfig()
        {
            // Arrange
            var spyAgent = new SpyAgent();
            var invalidConfig = new StubConfig(configSettings: null, sendingPModes: null, receivingPModes: null);
            var kernel = new Kernel(new[] { spyAgent }, invalidConfig);

            // Act
            await kernel.StartAsync(CancellationToken.None);

            // Assert
            Assert.False(spyAgent.HasStarted);
        }
    }
}
