using System.Collections;
using HarmonyLib;
using StardewValley;
using StardewValley.Network;

namespace Tests.HarmonyMocks;

public class HarmonyFarmerCollection
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Method(typeof(FarmerCollection), nameof(FarmerCollection.GetEnumerator)),
			prefix: new HarmonyMethod(typeof(HarmonyFarmerCollection), nameof(MockGetEnumerator))
		);
		
		harmony.Patch(
			AccessTools.Constructor(typeof(FarmerCollection.Enumerator), [typeof(GameLocation)]),
			prefix: new HarmonyMethod(typeof(HarmonyFarmerCollection), nameof(MockEnumeratorConstructor))
		);
		
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(FarmerCollection.Enumerator), nameof(FarmerCollection.Enumerator.Current)),
			prefix: new HarmonyMethod(typeof(HarmonyFarmerCollection), nameof(MockEnumeratorCurrent))
		);
		harmony.Patch(
			AccessTools.Method(typeof(FarmerCollection.Enumerator), nameof(FarmerCollection.Enumerator.MoveNext)),
			prefix: new HarmonyMethod(typeof(HarmonyFarmerCollection), nameof(MockEnumeratorMoveNext))
		);
	}
	
	public static void TearDown()
	{
		CollectionEnumerator = null;
	}

	public static IEnumerator CollectionEnumerator;

	static bool MockGetEnumerator() => false;
	static bool MockEnumeratorConstructor() => false;
	
	static bool MockEnumeratorCurrent(ref Farmer __result)
	{
		__result = CollectionEnumerator.Current as Farmer;
		return false;
	}
	
	static bool MockEnumeratorMoveNext(ref bool __result)
	{
		__result = CollectionEnumerator.MoveNext();
		return false;
	}
}
