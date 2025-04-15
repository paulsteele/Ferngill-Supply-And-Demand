using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;

public class HarmonyMapPage
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(MapPage), [typeof(int), typeof(int), typeof(int), typeof(int)]),
			prefix: new HarmonyMethod(typeof(HarmonyMapPage), nameof(MockConstructor))
		);
	}
	
	public static void TearDown()
	{
		// No static fields to reset
	}

	static bool MockConstructor() => false;
}
