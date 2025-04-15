using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;

public class HarmonyLetterViewMenu
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(LetterViewerMenu), [typeof(int)]),
			prefix: new HarmonyMethod(typeof(HarmonyLetterViewMenu), nameof(MockConstructor))
		);
	}
	
	public static void TearDown()
	{
		// No static fields to reset
	}

	static bool MockConstructor() => false;
}
