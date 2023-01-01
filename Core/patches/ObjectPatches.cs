using fsd.core.actions;
using HarmonyLib;
using StardewValley;

namespace fsd.core.patches
{
	public class ObjectPatches : SelfRegisteringPatches
	{
		public static void SellToStoreSalePricePostFix(Object __instance, ref int __result)
		{
			var res = __result;
			var newResult = SafeAction.Run(() =>
			{
				var newRes = res + 1000;
				return newRes;
			}, __result, Monitor);

			__result = newResult;
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