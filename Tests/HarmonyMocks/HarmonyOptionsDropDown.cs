using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;

public class HarmonyOptionsDropDown
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(OptionsDropDown), new []
			{
				typeof(string), 
				typeof(int),
				typeof(int),
				typeof(int)
			}),
			prefix: new HarmonyMethod(typeof(HarmonyOptionsDropDown), nameof(MockConstructor))
		);

		harmony.Patch
			(
				AccessTools.Method(typeof(OptionsDropDown), nameof(OptionsDropDown.RecalculateBounds)),
				prefix: new HarmonyMethod(typeof(HarmonyOptionsDropDown), nameof(MockRecalculateBounds))
			);
		
		harmony.Patch
			(
				AccessTools.Method(typeof(OptionsDropDown), nameof(OptionsDropDown.draw)),
				prefix: new HarmonyMethod(typeof(HarmonyOptionsDropDown), nameof(MockDraw))
			);

		ConstructorCalls = new();
		RecalculateCalls = new();
		DrawCalls = new();
	}
	
	public static void TearDown()
	{
		ConstructorCalls.Clear();
		RecalculateCalls.Clear();
		DrawCalls.Clear();
	}

	public static Dictionary<OptionsDropDown, List<(string label, int whichOption, int x, int y)>> ConstructorCalls;
	public static Dictionary<OptionsDropDown, int> RecalculateCalls;
	public static Dictionary<OptionsDropDown, int> DrawCalls;
	
	static bool MockConstructor
		(
			OptionsDropDown __instance,
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

	static bool MockRecalculateBounds(OptionsDropDown __instance)
	{
		RecalculateCalls.TryAdd(__instance, 0);
		RecalculateCalls[__instance]++;
		return false;
	}
	
	static bool MockDraw(OptionsDropDown __instance)
	{
		DrawCalls.TryAdd(__instance, 0);
		DrawCalls[__instance]++;
		return false;
	}
}
