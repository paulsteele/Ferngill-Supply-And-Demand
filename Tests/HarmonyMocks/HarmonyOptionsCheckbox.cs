using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;

public class HarmonyOptionsCheckbox
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(OptionsCheckbox), new []
			{
				typeof(string), 
				typeof(int),
				typeof(int),
				typeof(int),
			}),
			prefix: new HarmonyMethod(typeof(HarmonyOptionsCheckbox), nameof(MockConstructor))
		);

		harmony.Patch
			(
				AccessTools.Method(typeof(OptionsCheckbox), nameof(OptionsCheckbox.draw)),
				prefix: new HarmonyMethod(typeof(HarmonyOptionsCheckbox), nameof(MockDraw))
			);

		ConstructorCalls = new();
		DrawCalls = new();
	}
	
	public static void TearDown()
	{
		ConstructorCalls.Clear();
		DrawCalls.Clear();
	}

	public static Dictionary<OptionsCheckbox, List<(string label, int whichOption, int x, int y)>> ConstructorCalls;
	public static Dictionary<OptionsCheckbox, int> DrawCalls;
	
	static bool MockConstructor
		(
			OptionsCheckbox __instance,
			string label,
			int whichOption,
			int x,
			int y
		)
	{
		__instance.bounds = new Rectangle(x, y, 0, 0);
		if (!ConstructorCalls.ContainsKey(__instance))
		{
			ConstructorCalls.Add(__instance, []);
		}
		ConstructorCalls[__instance].Add((label, whichOption, x, y));
		return false;
	}
	
	static bool MockDraw(OptionsCheckbox __instance)
	{
		DrawCalls.TryAdd(__instance, 0);
		DrawCalls[__instance]++;
		return false;
	}
}
