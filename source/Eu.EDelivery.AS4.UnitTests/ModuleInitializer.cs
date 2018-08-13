using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.UnitTests
{
    /// <summary>
    /// Assembly initializer.
    /// </summary>
    public static class ModuleInitializer
    {
        /// <summary>
        /// Initializes the current assembly by calling this method when the assembly is loaded into memory.
        /// </summary>
        public static void Initialize()
        {
            AS4Mapper.Initialize();
        }
    }
}
