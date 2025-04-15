using System.Globalization;
using HarmonyLib;
using StardewValley;

namespace Tests.HarmonyMocks;

public class HarmonyLocalizedContentManager
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(LocalizedContentManager), new []
			{
				typeof(IServiceProvider), 
				typeof(string),
				typeof(CultureInfo),
			}),
			prefix: new HarmonyMethod(typeof(HarmonyLocalizedContentManager), nameof(MockConstructor))
		);

		harmony.Patch(
			typeof(LocalizedContentManager).GetMethods()
				.First(m => m.Name == nameof(LocalizedContentManager.Load) && m.GetParameters().Length == 1)
				.MakeGenericMethod(typeof(object)),
			prefix: new HarmonyMethod(typeof(HarmonyLocalizedContentManager), nameof(MockLoad))
		);
	}

	static bool MockConstructor() => false;
	
	public static object LoadResult { get; set; }
	static bool MockLoad(ref object __result)
	{
		__result = LoadResult;
		return false;
	}
}