using HarmonyLib;
using StardewValley;
using StardewValley.Network;

namespace Tests.HarmonyMocks;

public class HarmonyGame
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
}