using StardewModdingAPI;
using StardewValley;

namespace fsd.core.patches
{
    public class ShopMenuPatches : Patches
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
    }
}