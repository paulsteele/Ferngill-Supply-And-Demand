using HarmonyLib;
using StardewValley;
using StardewValley.Inventories;

namespace Tests.HarmonyMocks;

public static class HarmonyFarm
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(Farm)),
			prefix: new HarmonyMethod(typeof(HarmonyFarm), nameof(MockConstructor))
		);
		
		harmony.Patch(
			AccessTools.Method(typeof(Farm), nameof(Farm.getShippingBin)),
			prefix: new HarmonyMethod(typeof(HarmonyFarm), nameof(MockGetShippingBin))
		);
	}
	
	public static void TearDown()
	{
		GetShippingBinDictionary.Clear();
	}

	static bool MockConstructor() => false;
	
	public static Dictionary<Farmer, IInventory> GetShippingBinDictionary = new();
	
	static bool MockGetShippingBin(
		Farmer who,
		ref IInventory __result
	)
	{
		__result = GetShippingBinDictionary[who];
		return false;
	}
}
