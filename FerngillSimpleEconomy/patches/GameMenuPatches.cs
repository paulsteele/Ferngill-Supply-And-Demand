using System.Collections.Generic;
using System.Reflection.Emit;
using fse.core.menu;
using fse.core.models;
using HarmonyLib;
using Microsoft.Xna.Framework;
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