using StardewModdingAPI;

namespace fsd.core.handlers
{
    public abstract class SelfRegisteringHandler
    {
        protected static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }
        public abstract void Register(IModHelper helper);
    }
}