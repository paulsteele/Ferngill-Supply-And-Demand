using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;

public class HarmonyOptionsTextEntry
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(OptionsTextEntry), new []
			{
				typeof(string), 
				typeof(int),
				typeof(int),
				typeof(int),
			}),
			prefix: new HarmonyMethod(typeof(HarmonyOptionsTextEntry), nameof(MockConstructor))
		);

		harmony.Patch
			(
				AccessTools.Method(typeof(OptionsTextEntry), nameof(OptionsTextEntry.draw)),
				prefix: new HarmonyMethod(typeof(HarmonyOptionsTextEntry), nameof(MockDraw))
			);

		ConstructorCalls = new();
		DrawCalls = new();
	}

	public static Dictionary<OptionsTextEntry, List<(string label, int whichOption, int x, int y)>> ConstructorCalls;
	public static Dictionary<OptionsTextEntry, int> DrawCalls;
	
	static bool MockConstructor
		(
			OptionsTextEntry __instance,
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
		__instance.textBox = new TextBox(null, null, null, Color.Transparent);
		return false;
	}
	
	static bool MockDraw(OptionsTextEntry __instance)
	{
		DrawCalls.TryAdd(__instance, 0);
		DrawCalls[__instance]++;
		return false;
	}
}