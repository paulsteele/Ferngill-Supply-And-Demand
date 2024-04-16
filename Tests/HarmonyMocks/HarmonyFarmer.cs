using HarmonyLib;
using StardewValley;

namespace Tests.HarmonyMocks;

public class HarmonyFarmer
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(Farmer)),
			prefix: new HarmonyMethod(typeof(HarmonyFarmer), nameof(MockConstructor))
		);
		
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.UniqueMultiplayerID)),
			prefix: new HarmonyMethod(typeof(HarmonyFarmer), nameof(MockUniqueMultiplayerID))
		);
		
		UniqueMultiplayerIdDictionary.Clear();
	}

	static bool MockConstructor() => false;

	public static Dictionary<Farmer, long> UniqueMultiplayerIdDictionary = new();

	static bool MockUniqueMultiplayerID(
		Farmer __instance,
		ref long __result
	)
	{
		__result = UniqueMultiplayerIdDictionary[__instance];
		
		return false;
	}
}