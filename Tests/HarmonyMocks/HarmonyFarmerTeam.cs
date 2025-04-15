using System.Reflection;
using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;

namespace Tests.HarmonyMocks;

public static class HarmonyFarmerTeam
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(FarmerTeam)),
			prefix: new HarmonyMethod(typeof(HarmonyFarmerTeam), nameof(MockConstructor))
		);
	}
	
	public static void TearDown()
	{
		// No static fields to reset
	}

	static bool MockConstructor() => false;

	public static void SetUseSeparateWalletsResult(this FarmerTeam farmerTeam, bool value)
	{
		var field = AccessTools.Field(typeof(FarmerTeam), nameof(farmerTeam.useSeparateWallets));

		field.SetValue(farmerTeam, new NetBool(value), BindingFlags.Public, null, null);
	}
}
