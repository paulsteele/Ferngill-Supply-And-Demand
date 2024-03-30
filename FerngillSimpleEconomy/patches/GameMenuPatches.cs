using System.Collections.Generic;
using System.Reflection.Emit;
using fse.core.menu;
using fse.core.models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace fse.core.patches
{
	public class GameMenuPatches : SelfRegisteringPatches
	{
		public override void Register(Harmony harmony)
		{
			harmony.Patch(
				AccessTools.Constructor(typeof(GameMenu), new []{typeof(bool)}),
				transpiler: new HarmonyMethod(typeof(GameMenuPatches), nameof(TranspilerConstructor))
			);
			harmony.Patch(
				AccessTools.Method(typeof(GameMenu), nameof(GameMenu.getTabNumberFromName)),
				prefix: new HarmonyMethod(typeof(GameMenuPatches), nameof(PrefixGetTabIndex))
			);
			harmony.Patch(
				AccessTools.Method(typeof(GameMenu), nameof(GameMenu.changeTab)),
				postfix: new HarmonyMethod(typeof(GameMenuPatches), nameof(PostFixChangeTab))
			);
			harmony.Patch(
				AccessTools.Method(typeof(GameMenu), nameof(GameMenu.changeTab)),
				prefix: new HarmonyMethod(typeof(GameMenuPatches), nameof(PrefixChangeTab))
			);
			harmony.Patch(
				AccessTools.Method(typeof(GameMenu), nameof(GameMenu.draw), new []{typeof(SpriteBatch)}),
				transpiler: new HarmonyMethod(typeof(GameMenuPatches), nameof(TranspilerDraw))
			);
		}
		
		public static void PrefixChangeTab(GameMenu __instance, out int __state)
		{
			__state = __instance.currentTab;
		}

		// ReSharper disable once InconsistentNaming
		public static void PostFixChangeTab(GameMenu __instance, int __state)
		{
			if (__instance.currentTab != ConfigModel.Instance.MenuTabIndex)
			{
				return;
			}

			__instance.lastOpenedNonMapTab = __state;
			__instance.invisible = true;

			__instance.upperRightCloseButton.visible = false;
		}

		// ReSharper disable once InconsistentNaming
		public static bool PrefixGetTabIndex(ref int __result, ref string name)
		{
			if (name != "forecast")
			{
				return true;
			}

			__result = ConfigModel.Instance.MenuTabIndex;
			return false;
		}

		public static void AddForecastTab(
			// ReSharper disable once InconsistentNaming
			GameMenu __instance
		)
		{
			if (!ConfigModel.Instance.EnableMenuTab)
			{
				return;
			}
			
			__instance.pages.Add(new ForecastMenu(ModHelper, EconomyService, Monitor));
			__instance.tabs.Add(new ClickableComponent(
				new Rectangle(
					__instance.xPositionOnScreen + 64 * (ConfigModel.Instance.MenuTabIndex + 1), 
					__instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 
					64, 
					64
				), 
				"forecast", 
				ModHelper.Translation.Get("fse.forecast.menu.tab.title")
			)
			{
				myID = 12350,
				downNeighborID = 8,
				rightNeighborID = 12349,
				leftNeighborID = 12347,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
		}

		public static void DrawExtraTab(
			SpriteBatch b,
			// ReSharper disable once InconsistentNaming
			GameMenu __instance
		)
		{
			var tab = __instance.tabs[ConfigModel.Instance.MenuTabIndex];
			b.Draw(
				Game1.mouseCursors, 
				new Vector2(tab.bounds.X, tab.bounds.Y ), 
				new Rectangle?(new Rectangle(0 * 16, 368, 16, 16)),
				Color.White, 
				0.0f, 
				Vector2.Zero, 
				4f, 
				SpriteEffects.None, 
				0.0001f
			);
		}
		
		// ReSharper disable once InconsistentNaming
		// find the second
		// ldarg.1      // b
		// callvirt     instance void [MonoGame.Framework]Microsoft.Xna.Framework.Graphics.SpriteBatch::End()

		public static IEnumerable<CodeInstruction> TranspilerDraw(IEnumerable<CodeInstruction> steps)
		{
			using var enumerator = steps.GetEnumerator();
			var potential = false;
			var count = 0;
			
			while (enumerator.MoveNext())
			{
				var current = enumerator.Current;

				if (current.IsLdarg(1))
				{
					potential = true;
				}
				else if (potential && current.Calls(AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.End))))
				{
					count++;
					if (count == 2)
					{
						// don't need to do this since last instruction was this
						// yield return new CodeInstruction(OpCodes.Ldarg_1);
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GameMenuPatches), nameof(DrawExtraTab)));
						// need to put the batch back on the stack for the next instruction
						yield return new CodeInstruction(OpCodes.Ldarg_1);
					}
					else
					{
						potential = false;
					}
				}
				else
				{
					potential = false;
				}
				
				yield return enumerator.Current;
			}
		}

		// ReSharper disable once InconsistentNaming
		public static IEnumerable<CodeInstruction> TranspilerConstructor(IEnumerable<CodeInstruction> steps)
		{
			using var enumerator = steps.GetEnumerator();

			var tabsCount = 0;
			while (enumerator.MoveNext())
			{
				var current = enumerator.Current;
				if (current.Calls(AccessTools.Method(typeof(List<IClickableMenu>), nameof(List<IClickableMenu>.Add))))
				{
					tabsCount++;
					if (tabsCount == ConfigModel.Instance.MenuTabIndex)
					{
						yield return current;
						
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GameMenuPatches), nameof(AddForecastTab)));
						continue;
					}
				}
				
				yield return enumerator.Current;
			}
		}
	}
}