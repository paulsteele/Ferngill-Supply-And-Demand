using HarmonyLib;
using Object = StardewValley.Object;

namespace Tests.HarmonyMocks;

public class HarmonyObject
{
	public static void Setup(Harmony harmony)
	{
		// public Object(string itemId, int initialStack, bool isRecipe = false, int price = -1, int quality = 0)
		harmony.Patch(
			AccessTools.Constructor(typeof(Object), new []{typeof(string), typeof(int), typeof(bool), typeof(int), typeof(int)}),
			prefix: new HarmonyMethod(typeof(HarmonyObject), nameof(MockConstructor))
		);
	}

	static bool MockConstructor() => false;
}