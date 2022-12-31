using fsd.core.handlers;
using fsd.core.patches;
using fsd.core.services;
using HarmonyLib;
using StardewModdingAPI;

namespace fsd.core
{
    // ReSharper disable once UnusedType.Global
    public class FerngillSupplyAndDemandMod : Mod
    {
        private EconomyService _economyService;
        public override void Entry(IModHelper helper)
        {
            _economyService = new EconomyService(helper);
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
            new DayEndHandler(helper, Monitor).Register();
            new SaveLoadedHandler(helper, Monitor, _economyService).Register();
        }
    }
}