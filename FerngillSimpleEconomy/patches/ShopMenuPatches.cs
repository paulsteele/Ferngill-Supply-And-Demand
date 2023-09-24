using System;
using System.Collections.Generic;
using System.Linq;
using fse.core.models;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace fse.core.patches
{
	public class ShopMenuPatches : SelfRegisteringPatches
	{
		private static Dictionary<ISalable, ItemModel> ItemModels;

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

		public static void DrawSeedInfo(SpriteBatch b)
		{
			if (Game1.activeClickableMenu is not ShopMenu shopMenu)
			{
				return;
			}

			var buttonAndItem = shopMenu.forSaleButtons
				.Zip(shopMenu.forSale)
				.Where(t => t.Second is Object { Category: Object.SeedsCategory })
				.Select(t => (t.First, ItemModels.TryGetValue(t.Second, out var model) ? model : null))
				.Where(t => t.Item2 != null);

			foreach (var tuple in buttonAndItem)
			{
				Monitor.Log(tuple.Item2.DailyDelta.ToString(), LogLevel.Debug);
			}
		}

		public static void SetupShop(
			Dictionary<ISalable, int[]> itemPriceAndStock
		)
		{
			ItemModels = new Dictionary<ISalable, ItemModel>();
			var cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");

			foreach (var item in itemPriceAndStock)
			{
				if (item.Key is not Object obj)
				{
					continue;
				}

				if (!cropData.ContainsKey(obj.parentSheetIndex.Value))
				{
					continue;
				}

				var cropId = int.Parse(cropData[obj.parentSheetIndex.Value].Split('/')[3]);

				ItemModels.Add(item.Key, EconomyService.GetItemModel(cropId));
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
			
			harmony.Patch(
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.draw), new []{typeof(SpriteBatch)}),
				postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(DrawSeedInfo))
			);
			
			harmony.Patch(
				AccessTools.Constructor(typeof(ShopMenu),  new []
				{
					typeof(Dictionary<ISalable, int[]>),
					typeof(int),
					typeof(string),
					typeof(Func<ISalable, Farmer, int, bool>),
					typeof(Func<ISalable, bool>),
					typeof(string)
				}),
				postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(SetupShop))
			);
			
		}
	}
}