using HarmonyLib;
using StardewValley;
using StardewValley.Network;

namespace Tests.HarmonyMocks;

public static class HarmonyGame
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetPlayer))
		);
		harmony.Patch(
			AccessTools.Method(typeof(Game1), nameof(Game1.getOnlineFarmers)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetOnlineFarmers))
		);
		harmony.Patch(
			AccessTools.Method(typeof(Game1), nameof(Game1.getAllFarmers)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetAllFarmers))
		);
		harmony.Patch(
			AccessTools.Method(typeof(Game1), nameof(Game1.getFarm)),
			prefix: new HarmonyMethod(typeof(HarmonyGame), nameof(MockGetFarm))
		);
	}

	public static Farmer GetPlayerResult { get; set; }
	static bool MockGetPlayer(ref Farmer __result)
	{
		__result = GetPlayerResult;
		return false;
	}
	
	public static FarmerCollection GetOnlineFarmersResults { get; set; }
	static bool MockGetOnlineFarmers(ref FarmerCollection __result)
	{
		__result = GetOnlineFarmersResults;
		return false;
	}
	
	public static IEnumerable<Farmer> GetAllFarmersResults { get; set; }
	static bool MockGetAllFarmers(ref IEnumerable<Farmer> __result)
	{
		__result = GetAllFarmersResults;
		return false;
	}
	
	public static Farm GetFarmResult { get; set; }
	static bool MockGetFarm(ref Farm __result)
	{
		__result = GetFarmResult;
		return false;
	}
}