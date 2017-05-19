using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Agents
{
    public class SpyAgent : IAgent, IDisposable
    {
        private readonly EventWaitHandle _waitHandle = new ManualResetEvent(initialState: false);

        /// <summary>
        /// Gets the agent configuration.
        /// </summary>
        /// <value>
        /// The agent configuration.
        /// </value>
        public AgentConfig AgentConfig { get; } = null;

        /// <summary>
        /// Gets a value indicating whether this instance is stopped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is stopped; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is stopped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is stopped; otherwise, <c>false</c>.
        /// </value>
        public bool IsStopped { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has started; otherwise, <c>false</c>.
        /// </value>
        public bool HasStarted => _waitHandle.WaitOne(TimeSpan.FromSeconds(1));

        /// <summary>
        /// Starts the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task Start(CancellationToken cancellationToken)
        {
            _waitHandle.Set();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            IsStopped = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
        }

        [Fact]
        public void SpyOnDisposing()
        {
            // Arrange
            var sut = new SpyAgent();

            // Act
            sut.Dispose();

            // Assert
            Assert.True(sut.IsDisposed);
        }

        [Fact]
        public async Task SpyOnStarting()
        {
            // Arrange
            var sut = new SpyAgent();

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Assert.True(sut.HasStarted);
        }

        [Fact]
        public void SpyOnStopping()
        {
            // Arrange
            var sut = new SpyAgent();

            // Act
            sut.Stop();

            // Assert
            Assert.True(sut.IsStopped);
        }

        [Fact]
        public void SpyHasDefault()
        {
            Assert.Null(new SpyAgent().AgentConfig);
        }
    }
}
