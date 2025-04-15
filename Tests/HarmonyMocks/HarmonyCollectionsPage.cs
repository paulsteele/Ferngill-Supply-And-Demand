using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;

public class HarmonyCollectionsPage
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(CollectionsPage), [typeof(int), typeof(int), typeof(int), typeof(int)]),
			prefix: new HarmonyMethod(typeof(HarmonyCollectionsPage), nameof(MockConstructor))
		);
	}
	
	public static void TearDown()
	{
		// No static fields to reset
	}

	static bool MockConstructor() => false;
}
