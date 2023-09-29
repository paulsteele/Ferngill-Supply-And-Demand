using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using fse.core.menu;
using fse.core.models;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using Object = StardewValley.Object;

namespace fse.core.patches
{
	public class ShopMenuPatches : SelfRegisteringPatches
	{
		private static Dictionary<ISalable, ItemModel> _itemModels;
		private static ForecastMenu _forecastMenu;

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

		public static void DrawSeedInfo(
			SpriteBatch b,
			// ReSharper disable once InconsistentNaming
			ShopMenu __instance
		)
		{
			if (Game1.activeClickableMenu is not ShopMenu shopMenu)
			{
				return;
			}

			if (shopMenu.forSale == null)
			{
				return;
			}

			var items = shopMenu.forSale
				.Select((t, i) => (salable: t, visibleIndex: i - __instance.currentItemIndex))
				.Where(t => t.salable is Object { Category: Object.SeedsCategory })
				.Select(t => (model: _itemModels.TryGetValue(t.salable, out var model) ? model : null, t.visibleIndex))
				.Where(t => t.model != null)
				.Where(t => t.visibleIndex is >= 0 and < ShopMenu.itemsPerPage);

			foreach (var tuple in items)
			{
				var startingX = __instance.xPositionOnScreen + __instance.width - 400;
				var startingY = __instance.yPositionOnScreen + 16 + tuple.visibleIndex * ((__instance.height - 256) / 4);
				var width = 200;
				
				_forecastMenu.DrawRow(b, tuple.model, 0, startingX, startingY, width, 0, 0, false);
			}
		}

		public static void SetupShop(
			Dictionary<ISalable, int[]> itemPriceAndStock
		)
		{
			_itemModels = new Dictionary<ISalable, ItemModel>();
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

				_itemModels.Add(item.Key, EconomyService.GetItemModel(cropId));
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
				transpiler: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopDrawingTranspiler))
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

			_forecastMenu = new ForecastMenu(EconomyService, Monitor);
		}

		public static IEnumerable<CodeInstruction> ShopDrawingTranspiler(IEnumerable<CodeInstruction> steps)
		{
			using var enumerator = steps.GetEnumerator();
			
			while (enumerator.MoveNext())
			{
				var current = enumerator.Current;


				if (current?.opcode == OpCodes.Ldfld && (FieldInfo)current?.operand == AccessTools.Field(typeof(ShopMenu), nameof(ShopMenu.downArrow)))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopMenuPatches), nameof(DrawSeedInfo)));
				}
				
				yield return enumerator.Current;
			}
		}
	}
}