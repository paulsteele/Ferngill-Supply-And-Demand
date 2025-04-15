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
		
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.IsMainPlayer)),
			prefix: new HarmonyMethod(typeof(HarmonyFarmer), nameof(MockIsMainPlayer))
		);
		
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.team)),
			prefix: new HarmonyMethod(typeof(HarmonyFarmer), nameof(MockGetTeam))
		);

		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Gender)),
			prefix: new HarmonyMethod(typeof(HarmonyFarmer), nameof(MockGetGender))

		);
		
		UniqueMultiplayerIdDictionary.Clear();
	}
	
	public static void TearDown()
	{
		UniqueMultiplayerIdDictionary.Clear();
		IsMainPlayerDictionary.Clear();
		FarmerTeamDictionary.Clear();
	}

	static bool MockConstructor() => false;

	public static Dictionary<Farmer, long> UniqueMultiplayerIdDictionary = new();
	public static Dictionary<Farmer, bool> IsMainPlayerDictionary = new();
	public static Dictionary<Farmer, FarmerTeam> FarmerTeamDictionary = new();

	static bool MockUniqueMultiplayerID(
		Farmer __instance,
		ref long __result
	)
	{
		__result = UniqueMultiplayerIdDictionary[__instance];
		return false;
	}
	
	static bool MockIsMainPlayer(
		Farmer __instance,
		ref bool __result
	)
	{
		__result = IsMainPlayerDictionary[__instance];
		
		return false;
	}
	
	static bool MockGetTeam(
		Farmer __instance,
		ref FarmerTeam __result
	)
	{
		__result = FarmerTeamDictionary[__instance];
		
		return false;
	}
	
	static bool MockGetGender(
		Farmer __instance,
		ref Gender __result
	)
	{
		__result = Gender.Undefined;
		
		return false;
	}
}
