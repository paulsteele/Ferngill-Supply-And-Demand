using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Menus;

namespace fse.core.patches
{
	public class GameMenuPatches : SelfRegisteringPatches
	{
		public override void Register(Harmony harmony)
		{
			harmony.Patch(
				AccessTools.Constructor(typeof(GameMenu), new []{typeof(bool)}),
				transpiler: new HarmonyMethod(typeof(GameMenuPatches), nameof(GameMenuConstructor))
			);
		}


		public static void AddForecastTab(
			// ReSharper disable once InconsistentNaming
			ShopMenu __instance
		)
		{
			Monitor.Log("write forecast tab here");
			Monitor.Log(__instance.ToString());
		}

		// ReSharper disable once InconsistentNaming
		public static IEnumerable<CodeInstruction> GameMenuConstructor(IEnumerable<CodeInstruction> steps)
		{
			using var enumerator = steps.GetEnumerator();

			var tabsCount = 0;
			while (enumerator.MoveNext())
			{
				var current = enumerator.Current;
				if (current.Calls(AccessTools.Method(typeof(List<IClickableMenu>), nameof(List<IClickableMenu>.Add))))
				{
					tabsCount++;
					if (tabsCount == 10)
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