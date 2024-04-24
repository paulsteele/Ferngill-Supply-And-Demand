using fse.core.multiplayer;
using HarmonyLib;
using StardewModdingAPI.Events;

namespace Tests.HarmonyMocks;

public class HarmonyModMessageReceivedEventArgs
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(ModMessageReceivedEventArgs)),
			prefix: new HarmonyMethod(typeof(HarmonyModMessageReceivedEventArgs), nameof(MockConstructor))
		);
		
		harmony.Patch(
			typeof(ModMessageReceivedEventArgs)
				.GetMethod(nameof(ModMessageReceivedEventArgs.ReadAs))!
				.MakeGenericMethod(typeof(IMessage)),
			prefix: new HarmonyMethod(typeof(HarmonyModMessageReceivedEventArgs), nameof(MockReadAs))
		);
	}
	
	static bool MockConstructor() => false;
	
	public static IMessage ReadAsMessage { get; set; }
	static bool MockReadAs(
		ref object __result
	)
	{
		__result = ReadAsMessage;
		return false;
	}
}