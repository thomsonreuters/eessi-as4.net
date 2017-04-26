using System;

namespace Eu.EDelivery.AS4.VolumeTests
{
    /// <summary>
    /// Bridge to add an extra abstraction layer for the AS4 Corner creation/destruction.
    /// </summary>
    public class VolumeTestBridge : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTestBridge" /> class.
        /// </summary>
        public VolumeTestBridge()
        {
            Corner2 = Corner.StartNew("c2");
            Corner3 = Corner.StartNew("c3");
        }

        /// <summary>
        /// Gets the facade for the AS4 Corner 2 instance.
        /// </summary>
        protected Corner Corner2 { get; }

        /// <summary>
        /// Gets the facade for the AS4 Corner 3 instance.
        /// </summary>
        protected Corner Corner3 { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Corner2.Dispose();
            Corner3.Dispose();
        }
    }
}