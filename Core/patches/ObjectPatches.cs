using fsd.core.actions;

namespace fsd.core.patches
{
    public class ObjectPatches : Patches
    {
        public static void SellToStoreSalePricePostFix(StardewValley.Object __instance, ref int __result)
        {
            var res = __result;
            var newResult = SafeAction.Run(() =>
            {
                var newRes = res + 1000;
                return newRes;
            }, __result, Monitor);

            __result = newResult;
        }
    }
}