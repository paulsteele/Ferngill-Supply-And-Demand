using System.Globalization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using Object = StardewValley.Object;

namespace Tests.HarmonyMocks;

public class HarmonyItem
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Item), nameof(Item.Category)),
			prefix: new HarmonyMethod(typeof(HarmonyItem), nameof(MockGetCategory))
		);
		harmony.Patch(
			AccessTools.PropertySetter(typeof(Item), nameof(Item.Category)),
			prefix: new HarmonyMethod(typeof(HarmonyItem), nameof(MockSetCategory))
		);

		CategoryMapping = new();
	}
	
	public static void TearDown()
	{
		CategoryMapping.Clear();
	}

	public static Dictionary<Item, int> CategoryMapping;

	static bool MockGetCategory(
		ref Object __instance,
		ref int __result
	)
	{
		__result = CategoryMapping.GetValueOrDefault(__instance, -1);
		return false;
	}
	
	static bool MockSetCategory(
		ref Object __instance,
		int value
	)
	{
		CategoryMapping.TryAdd(__instance, value);
		CategoryMapping[__instance] = value;
		return false;
	}

}
