using StardewModdingAPI;

namespace fsd.core.patches
{
    public abstract class Patches
    {
        protected static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }
    }
}