using fsd.core.patches;
using HarmonyLib;
using StardewModdingAPI;
using Patches = fsd.core.patches.Patches;

namespace fsd.core
{
    public class FerngillSupplyAndDemandMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Patches.Initialize(Monitor);
            
            var harmony = new Harmony(ModManifest.UniqueID);
            
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.sellToStorePrice)),
                postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.SellToStoreSalePricePostFix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.AddBuybackItem)),
                prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopMenuPatches.AddBuyBackItemPreFix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.BuyBuybackItem)),
                postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopMenuPatches.BuyBuybackItemPostFix))
            );
        }
    }
}