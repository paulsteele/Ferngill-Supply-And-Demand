using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;

namespace Tests.HarmonyMocks;

public static class HarmonyOptions
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(Options)),
			prefix: new HarmonyMethod(typeof(HarmonyOptions), nameof(MockConstructor))
		);
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Options), nameof(Options.uiScale)),
			prefix: new HarmonyMethod(typeof(HarmonyOptions), nameof(MockGetUiScale))
		);
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Options), nameof(Options.zoomLevel)),
			prefix: new HarmonyMethod(typeof(HarmonyOptions), nameof(MockGetZoomLevel))
		);
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Options), nameof(Options.hardwareCursor)),
			prefix: new HarmonyMethod(typeof(HarmonyOptions), nameof(MockGetHardwareCursor))
		);

		GetUiScaleResult = 1f;
		GetZoomLevelResult = 1f;
		GetHardwareCursor = false;
	}
	
	public static void TearDown()
	{
		GetUiScaleResult = 1f;
		GetZoomLevelResult = 1f;
		GetHardwareCursor = false;
	}

	public static float GetUiScaleResult { get; set; } = 1f;
	public static float GetZoomLevelResult { get; set; } = 1f;
	public static bool GetHardwareCursor { get; set; } = false;
	
	static bool MockConstructor() => false;

	static bool MockGetUiScale(ref float __result)
	{
		__result = GetUiScaleResult;
		return false;
	}
	static bool MockGetZoomLevel(ref float __result)
	{
		__result = GetZoomLevelResult;
		return false;
	}
	static bool MockGetHardwareCursor(ref bool __result)
	{
		__result = GetHardwareCursor;
		return false;
	}
}
