using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Tests.HarmonyMocks;

public class HarmonySpriteBatch
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(SpriteBatch), new []{typeof(GraphicsDevice), typeof(int)}),
			prefix: new HarmonyMethod(typeof(HarmonySpriteBatch), nameof(MockConstructor))
		);
	}
	
	static bool MockConstructor() => false;
}