using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace fsd.core.patches
{
    public class ShopMenuPatches : SelfRegisteringPatches
    {
        //Prefix as the number of sold stacks is modified in the original function
        public static bool AddBuyBackItemPreFix(ISalable sold_item, int sell_unit_price, int stack)
        {
            Monitor.Log($"sold {sold_item.Name} at {sell_unit_price} {stack}x", LogLevel.Error);
            return true;
        }
        
        public static void BuyBuybackItemPostFix(ISalable bought_item, int price, int stack)
        {
            Monitor.Log($"buyback {bought_item.Name} at {price} {stack}x", LogLevel.Error);
        }

        public override void Register(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.AddBuybackItem)),
                prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(AddBuyBackItemPreFix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.BuyBuybackItem)),
                postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(BuyBuybackItemPostFix))
            );
        }
    }
}