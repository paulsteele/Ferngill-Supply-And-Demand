using fsd.core.handlers;
using fsd.core.patches;
using HarmonyLib;
using StardewModdingAPI;

namespace fsd.core
{
    // ReSharper disable once UnusedType.Global
    public class FerngillSupplyAndDemandMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            RegisterPatches();
            RegisterHandlers(helper);
        }

        private void RegisterPatches()
        {
            var harmony = new Harmony(ModManifest.UniqueID);
            
            SelfRegisteringPatches.Initialize(Monitor);
            new ObjectPatches().Register(harmony);
            new ShopMenuPatches().Register(harmony);
        }

        private void RegisterHandlers(IModHelper helper)
        {
            SelfRegisteringHandler.Initialize(Monitor);
            new DayEndHandler().Register(helper);
        }
    }
}