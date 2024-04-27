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

		GetUiScaleResult = 1f;
		GetZoomLevelResult = 1f;
	}

	static bool MockConstructor() => false;

	public static float GetUiScaleResult { get; set; } = 1f;
	static bool MockGetUiScale(ref float __result)
	{
		__result = GetUiScaleResult;
		return false;
	}
	public static float GetZoomLevelResult { get; set; } = 1f;
	static bool MockGetZoomLevel(ref float __result)
	{
		__result = GetZoomLevelResult;
		return false;
	}
}