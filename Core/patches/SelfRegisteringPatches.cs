using HarmonyLib;
using StardewModdingAPI;

namespace fsd.core.patches
{
    public abstract class SelfRegisteringPatches
    {
        protected static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public abstract void Register(Harmony harmony);
    }
}