using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tests.HarmonyMocks;

public class HarmonyGraphicsDeviceManager
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(GraphicsDeviceManager), [typeof(Game)]),
			prefix: new HarmonyMethod(typeof(HarmonyGraphicsDeviceManager), nameof(MockConstructor))
		);
	}

	static bool MockConstructor() => false;
}