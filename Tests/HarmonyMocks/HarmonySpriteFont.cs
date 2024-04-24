using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Tests.HarmonyMocks;

public class HarmonySpriteFont
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(SpriteFont)),
			prefix: new HarmonyMethod(typeof(HarmonySpriteFont), nameof(MockConstructor))
		);
		
		harmony.Patch(
			AccessTools.Method(typeof(SpriteFont), nameof(SpriteFont.MeasureString), new []{typeof(string)}),
			prefix: new HarmonyMethod(typeof(HarmonySpriteFont), nameof(MockConstructor))
		);
	}
	
	static bool MockConstructor() => false;
	
	public static Vector2 MeasureStringResult { get; set; }
	static bool MockMeasureString(ref Vector2 __result)
	{
		__result = MeasureStringResult;
		return false;
	}
}