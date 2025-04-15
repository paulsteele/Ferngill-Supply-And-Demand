using HarmonyLib;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;
public class HarmonyExitPage
{	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(ExitPage), [typeof(int), typeof(int), typeof(int), typeof(int)]),
			prefix: new HarmonyMethod(typeof(HarmonyExitPage), nameof(MockConstructor))
		);
		harmony.Patch(
			AccessTools.Constructor(typeof(ExitPage)),
			prefix: new HarmonyMethod(typeof(HarmonyExitPage), nameof(MockConstructor))
		);
	}

	static bool MockConstructor() => false;
}