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
		
		harmony.Patch(
			AccessTools.Method(typeof(Object), nameof(Object.getCategoryName)),
			prefix: new HarmonyMethod(typeof(HarmonyObject), nameof(MockGetCategoryName))
		);

		DrawInMenuCalls = new();
		ObjectIdCategoryMapping = new();
		CategoryIdToNameMapping = new();
		ObjectIdToPriceMapping = new();
		SellToStorePriceMapping = new();
	}

	public static Dictionary<Object, List<Vector2>> DrawInMenuCalls;
	public static Dictionary<string, int> ObjectIdCategoryMapping;
	public static Dictionary<int, string> CategoryIdToNameMapping;
	public static Dictionary<string, int> ObjectIdToPriceMapping;
	public static Dictionary<Object, int> SellToStorePriceMapping;

	static bool MockConstructor(
		ref Object __instance,
		string itemId,
		int initialStack
		)
	{
		__instance.itemId = new NetString();
		__instance.ItemId = itemId;
		if (ObjectIdCategoryMapping.TryGetValue(itemId, out var category))
		{
			__instance.Category = category;
		}
		
		#pragma warning disable AvoidNetField
		var stackField = AccessTools.Field(typeof(Object), nameof(Object.stack));
		#pragma warning restore AvoidNetField
		stackField.SetValue(__instance, new NetInt(initialStack));

		if (ObjectIdToPriceMapping.TryGetValue(itemId, out var price))
		{
			#pragma warning disable AvoidNetField
			var priceField = AccessTools.Field(typeof(Object), nameof(Object.price));
			#pragma warning restore AvoidNetField
			priceField.SetValue(__instance, new NetInt(price));
		}

		var parentField = AccessTools.Field(typeof(Object), nameof(Object.preservedParentSheetIndex));
		parentField.SetValue(__instance, new NetString());
		
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
	
	static bool MockGetCategoryName(
		ref Object __instance,
		ref string __result
		)
	{
		if (CategoryIdToNameMapping.TryGetValue(__instance.Category, out var name))
		{
			__result = name;
		}
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