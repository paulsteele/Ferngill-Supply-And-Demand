using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace fsd.core.patches
{
	public class ShopMenuPatches : SelfRegisteringPatches
	{
		//Prefix as the number of sold stacks is modified in the original function
		public static bool AddBuyBackItemPreFix(ISalable sold_item, int sell_unit_price, int stack)
		{
			if (sold_item is Object soldObject)
			{
				EconomyService.AdjustSupply(soldObject, stack);
			}
			return true;
		}

		public static void BuyBuybackItemPostFix(ISalable bought_item, int price, int stack)
		{
			if (bought_item is Object boughtObject)
			{
				EconomyService.AdjustSupply(boughtObject, -stack);
			}
		}

		public override void Register(Harmony harmony)
		{
			harmony.Patch(
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.AddBuybackItem)),
				new HarmonyMethod(typeof(ShopMenuPatches), nameof(AddBuyBackItemPreFix))
			);

			harmony.Patch(
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.BuyBuybackItem)),
				postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(BuyBuybackItemPostFix))
			);
		}
	}
}