using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using fse.core.models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace fse.core.patches
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

		public static void DrawSeedInfo(
			SpriteBatch b,
			// ReSharper disable once InconsistentNaming
			ShopMenu shopMenu
		)
		{
			if (shopMenu.forSale == null)
			{
				return;
			}

			if (!ConfigModel.Instance.EnableShopDisplay)
			{
				return;
			}

			var forSaleButtons = shopMenu.forSaleButtons;
			var forSale = shopMenu.forSale;
			int currItemIdx = shopMenu.currentItemIndex;
			int vanillaBtnWidth = shopMenu.width - 32;
			for (int i = 0; i < forSaleButtons.Count; i++)
			{
				if (currItemIdx + i >= forSale.Count)
				{
					continue;
				}
				ClickableComponent component = forSaleButtons[i];
				ISalable salable = forSale[currItemIdx + i];
				Rectangle bounds = component.bounds;
				if (salable is Object { Category: Object.SeedsCategory } obj && EconomyService.GetItemModelFromSeed(obj.ItemId) is ItemModel model)
				{
					if (bounds.Width < vanillaBtnWidth)
					{
						DrawSupplyBarHelper.DrawSupplyBar(b, bounds.X + 96, component.bounds.Y + 20, bounds.Right - bounds.Width / 5, 30, model);
					}
					else
					{
						DrawSupplyBarHelper.DrawSupplyBar(b, bounds.Right - 400, component.bounds.Y + 20, bounds.Right - 200, 30, model);
					}
				}
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
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.draw), new[] { typeof(SpriteBatch) }),
				transpiler: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopDrawingTranspiler))
			);
		}

		public static IEnumerable<CodeInstruction> ShopDrawingTranspiler(IEnumerable<CodeInstruction> steps)
		{
			using var enumerator = steps.GetEnumerator();

			while (enumerator.MoveNext())
			{
				var current = enumerator.Current;

				if (current.opcode == OpCodes.Ldfld && (FieldInfo)current.operand == AccessTools.Field(typeof(ShopMenu), nameof(ShopMenu.downArrow)))
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