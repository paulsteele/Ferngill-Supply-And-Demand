using fse.core.models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Network;
using StardewValley.SpecialOrders;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace fse.core.patches
{
	public class Game1Patches() : SelfRegisteringPatches
	{
		public override void Register(Harmony harmony)
		{
			if (ConfigModel.Instance.EnabledDynamicEcon)
			{

				System.Type? stateMachine = typeof(Game1).GetNestedType("<_newDayAfterFade>d__784", BindingFlags.NonPublic);
				MethodInfo? moveNext = stateMachine?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);

				harmony.Patch(
					original: moveNext,
					transpiler: new HarmonyMethod(typeof(Game1Patches), nameof(SellShippingBinItemsTranspiler))
				);
			}
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



		public static IEnumerable<int> PreFixPatch()
		{
			NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>> additional_shipped_items = new NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>();
			if (Game1.IsMasterGame)
			{
				Utility.ForEachLocation(delegate (GameLocation location)
				{
					foreach (StardewValley.Object value in location.objects.Values)
					{
						if (value is StardewValley.Objects.Chest { SpecialChestType: StardewValley.Objects.Chest.SpecialChestTypes.MiniShippingBin } chest)
						{
							chest.clearNulls();
							if (Game1.player.team.useSeparateWallets.Value)
							{
								foreach (long current in chest.separateWalletItems.Keys)
								{
									if (!additional_shipped_items.ContainsKey(current))
									{
										additional_shipped_items[current] = new NetList<Item, NetRef<Item>>();
									}
									List<Item> list = new List<Item>(chest.separateWalletItems[current]);
									chest.separateWalletItems[current].Clear();
									foreach (Item current2 in list)
									{
										current2.onDetachedFromParent();
										additional_shipped_items[current].Add(current2);
									}
								}
							}
							else
							{
								IInventory shippingBin = Game1.getFarm().getShippingBin(Game1.player);
								shippingBin.RemoveEmptySlots();
								foreach (Item current3 in chest.Items)
								{
									current3.onDetachedFromParent();
									shippingBin.Add(current3);
								}
							}
							chest.Items.Clear();
							chest.separateWalletItems.Clear();
						}
					}
					return true;
				});
			}


			if (Game1.IsMasterGame)
			{
				Game1.newDaySync.sendVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items", additional_shipped_items);
			}
			else
			{
				while (!Game1.newDaySync.isVarReady("additional_shipped_items"))
				{
					yield return 0;
				}
				additional_shipped_items = Game1.newDaySync.waitForVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items");
			}

			if (Game1.player.team.useSeparateWallets.Value)
			{
				IInventory shipping_bin = Game1.getFarm().getShippingBin(Game1.player);
				if (additional_shipped_items.TryGetValue(Game1.player.UniqueMultiplayerID, out var item_list))
				{
					foreach (Item item in item_list)
					{
						shipping_bin.Add(item);
					}
				}
			}

			Game1.newDaySync.barrier("handleMiniShippingBins");
			while (!Game1.newDaySync.isBarrierReady("handleMiniShippingBins"))
			{
				yield return 0;
			}


			IInventory shippingBin2 = Game1.getFarm().getShippingBin(Game1.player);
			shippingBin2.RemoveEmptySlots();
			foreach (Item i3 in shippingBin2)
			{
				Game1.player.displayedShippedItems.Add(i3);
			}
			if (Game1.player.useSeparateWallets || Game1.player.IsMainPlayer)
			{
				int total = 0;
				foreach (Item item2 in shippingBin2)
				{
					int item_value = 0;
					if (item2 is StardewValley.Object obj)
					{
						item_value = obj.sellToStorePrice(-1L) * obj.Stack;
						total += item_value;
					}
					//if (Game1.player.team.specialOrders == null)
					//{
					//	continue;
					//}
					foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
					{
						specialOrder.onItemShipped?.Invoke(Game1.player, item2, item_value);
					}
				}
				Game1.player.Money += total;
			}
			if (Game1.IsMasterGame)
			{
				if (Game1.IsWinter && Game1.dayOfMonth == 18)
				{
					GameLocation source = Game1.RequireLocation("Submarine");
					if (source.objects.Length >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(source, null, new Vector2(20f, 20f), Game1.getLocationFromName("Beach"));
					}
					source = Game1.RequireLocation("MermaidHouse");
					if (source.objects.Length >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(source, null, new Vector2(21f, 20f), Game1.getLocationFromName("Beach"));
					}
				}
				if (Game1.player.hasOrWillReceiveMail("pamHouseUpgrade") && !Game1.player.hasOrWillReceiveMail("transferredObjectsPamHouse"))
				{
					Game1.addMailForTomorrow("transferredObjectsPamHouse", noLetter: true);
					GameLocation source2 = Game1.RequireLocation("Trailer");
					GameLocation destination = Game1.getLocationFromName("Trailer_Big");
					if (source2.objects.Length >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(source2, destination, new Vector2(14f, 23f));
					}
				}
				if (Utility.HasAnyPlayerSeenEvent("191393") && !Game1.player.hasOrWillReceiveMail("transferredObjectsJojaMart"))
				{
					Game1.addMailForTomorrow("transferredObjectsJojaMart", noLetter: true);
					GameLocation source3 = Game1.RequireLocation("JojaMart");
					if (source3.objects.Length >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(source3, null, new Vector2(89f, 51f), Game1.getLocationFromName("Town"));
					}
				}
			}
			if (Game1.player.useSeparateWallets && Game1.player.IsMainPlayer)
			{
				foreach (Farmer who in Game1.getOfflineFarmhands())
				{
					if (who.isUnclaimedFarmhand)
					{
						continue;
					}
					int total2 = 0;
					IInventory farmhandShippingBin = Game1.getFarm().getShippingBin(who);
					farmhandShippingBin.RemoveEmptySlots();
					foreach (Item item3 in farmhandShippingBin)
					{
						int item_value2 = 0;
						if (item3 is StardewValley.Object obj2)
						{
							item_value2 = obj2.sellToStorePrice(who.UniqueMultiplayerID) * obj2.Stack;
							total2 += item_value2;
						}
						//if (Game1.player.team.specialOrders == null)
						//{
						//	continue;
						//}
						foreach (SpecialOrder specialOrder2 in Game1.player.team.specialOrders)
						{
							specialOrder2.onItemShipped?.Invoke(Game1.player, item3, item_value2);
						}
					}
					Game1.player.team.AddIndividualMoney(who, total2);
					farmhandShippingBin.Clear();
				}
			}
		}
	}
}