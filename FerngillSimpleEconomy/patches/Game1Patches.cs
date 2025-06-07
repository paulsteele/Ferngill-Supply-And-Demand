using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace fse.core.patches
{
	public class Game1Patches() : SelfRegisteringPatches
	{
		public override void Register(Harmony harmony)
		{
			System.Type? stateMachine = typeof(Game1).GetNestedType("<_newDayAfterFade>d__784", BindingFlags.NonPublic);
			MethodInfo? moveNext = stateMachine?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);

			harmony.Patch(
				original: moveNext,
				transpiler: new HarmonyMethod(typeof(Game1Patches), nameof(SellShippingBinItemsTranspiler))
			);
		}


		public static IEnumerable<CodeInstruction> SellShippingBinItemsTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			try
			{
				MethodInfo itemSellToStorePriceMethInfo = AccessTools.Method(typeof(Item), nameof(Item.sellToStorePrice));
				MethodInfo getStackMethInfo = AccessTools.PropertyGetter(typeof(Item), nameof(Item.Stack));
				MethodInfo SellingPatchMethInfo = AccessTools.Method(typeof(Game1Patches), nameof(SellShippingBinItem));

				CodeMatcher matcher = new CodeMatcher(instructions);

				//From: obj.sellToStorePrice(playerId) * obj.Stack;
				//To: SellShippingBinItem(obj, playerId);
				matcher
				.MatchStartForward(
					new CodeMatch(OpCodes.Callvirt, itemSellToStorePriceMethInfo),  //get item price
					new CodeMatch(OpCodes.Ldloc_S),                                 //store result
					new CodeMatch(OpCodes.Callvirt, getStackMethInfo),              //get stack amount
					new CodeMatch(OpCodes.Mul)                                      //mul
				).ThrowIfNotMatch("IL code not found: Failed to patch shippingBin2")
					.RemoveInstructions(4)
					.InsertAndAdvance( new CodeInstruction(OpCodes.Call, SellingPatchMethInfo) );

				matcher.MatchStartForward(
					new CodeMatch(OpCodes.Callvirt, itemSellToStorePriceMethInfo),  //get item price
					new CodeMatch(OpCodes.Ldloc_S),                                 //store result
					new CodeMatch(OpCodes.Callvirt, getStackMethInfo),              //get stack amount
					new CodeMatch(OpCodes.Mul)                                      //mul
				)
				.ThrowIfNotMatch("IL code not found: Failed to patch shippingBin4")
				.RemoveInstructions(4)
				.InsertAndAdvance(
						new CodeInstruction(OpCodes.Call, SellingPatchMethInfo)
					);

				return matcher.InstructionEnumeration();
			}
			catch (System.Exception ex)
			{
				Monitor.Log($"Failed in {nameof(SellShippingBinItemsTranspiler)}:\n{ex}", LogLevel.Error);
				return instructions;
			}	
		}


		//TODO this method only patches out the Pay to Player shipping amount, leaving the Shipping summary screen showing incorrect values
		public static int SellShippingBinItem(Item item, long playerId)
		{
			int price = item.sellToStorePrice(playerId);
			//TODO use logger and log something meaning full
			System.Console.WriteLine(", Item_name: " + item.BaseName + ", Stack: " + item.Stack +  ", Price: "+ price + ", playerId: " + playerId);
			if (item is Object obj)
			{
				EconomyService.AdjustSupply(obj, obj.Stack, true, true);
			}
			return price * item.Stack;
		}
	}
}