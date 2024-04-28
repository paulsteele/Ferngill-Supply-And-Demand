using System.Globalization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using Object = StardewValley.Object;

namespace Tests.HarmonyMocks;

public class HarmonyObject
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(Object), new []{typeof(string), typeof(int), typeof(bool), typeof(int), typeof(int)}),
			prefix: new HarmonyMethod(typeof(HarmonyObject), nameof(MockConstructor))
		);
		
		harmony.Patch(
			AccessTools.Method(typeof(Item), nameof(Object.drawInMenu), new []
			{
				typeof(SpriteBatch),
				typeof(Vector2),
				typeof(float),
			}),
			prefix: new HarmonyMethod(typeof(HarmonyObject), nameof(MockDrawInMenu))
		);
		
		harmony.Patch(
			AccessTools.PropertyGetter(typeof(Object), nameof(Object.DisplayName)),
			prefix: new HarmonyMethod(typeof(HarmonyObject), nameof(MockGetDisplayName))
		);
		
		harmony.Patch(
			AccessTools.Method(typeof(Object), nameof(Object.sellToStorePrice)),
			prefix: new HarmonyMethod(typeof(HarmonyObject), nameof(MockSellToStorePrice))
		);

		DrawInMenuCalls = new();
	}

	public static Dictionary<Object, List<Vector2>> DrawInMenuCalls;

	static bool MockConstructor(
		ref Object __instance,
		string itemId
		)
	{
		__instance.itemId = new NetString();
		__instance.ItemId = itemId;
		return false;
	}
	
	static bool MockDrawInMenu(
		ref Object __instance,
		Vector2 location
		)
	{

		if (!DrawInMenuCalls.ContainsKey(__instance))
		{
			DrawInMenuCalls.Add(__instance, []);
		}
		
		DrawInMenuCalls[__instance].Add(location);
		
		return false;
	}
	static bool MockGetDisplayName(
		ref Object __instance,
		ref string __result
		)
	{
		__result = $"display-{__instance.ItemId}";
		return false;
	}
	
	static bool MockSellToStorePrice(
		ref Object __instance,
		ref int __result
		)
	{
		if (int.TryParse(__instance.ItemId, out var res))
		{
			__result = res;
		}
		else
		{
			__result = -1;
		}
		
		return false;
	}
}