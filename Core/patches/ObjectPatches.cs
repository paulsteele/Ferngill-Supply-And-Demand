using fsd.core.actions;
using HarmonyLib;
using StardewValley;

namespace fsd.core.patches
{
	public class ObjectPatches : SelfRegisteringPatches
	{
		public static void SellToStoreSalePricePostFix(Object __instance, ref int __result)
		{
			var basePrice = __result;
			__result = SafeAction.Run(() => EconomyService.GetPrice(__instance, basePrice), __result, Monitor);
		}

		public override void Register(Harmony harmony)
		{
			harmony.Patch(
				AccessTools.Method(typeof(Object), nameof(Object.sellToStorePrice)),
				postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(SellToStoreSalePricePostFix))
			);
		}
	}
}