using Microsoft.AspNetCore.Builder;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    /// <summary>
    /// Setup settings at config time
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.ILifeCycleHook" />
    public interface IRunAtAppConfiguration : ILifeCycleHook
    {
        /// <summary>
        /// Setup at configuration time
        /// </summary>
        /// <param name="app">The application.</param>
        void Run(IApplicationBuilder app);
    }
}