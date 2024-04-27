using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;

namespace Tests.HarmonyMocks;

public static class HarmonyGameMenu
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(GameMenu), [typeof(int), typeof(int), typeof(bool)]),
			prefix: new HarmonyMethod(typeof(HarmonyGameMenu), nameof(MockConstructor))
		);
		harmony.Patch(
			AccessTools.Constructor(typeof(GameMenu), [typeof(bool)]),
			prefix: new HarmonyMethod(typeof(HarmonyGameMenu), nameof(MockConstructor))
		);
	}

	static bool MockConstructor() => false;
}