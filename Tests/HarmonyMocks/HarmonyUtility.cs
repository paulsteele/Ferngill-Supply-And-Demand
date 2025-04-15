using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Network;

namespace Tests.HarmonyMocks;

public static class HarmonyUtility
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Method(
				typeof(Utility), 
				nameof(Utility.drawTextWithShadow),
				new []
				{
					typeof(SpriteBatch),
					typeof(string),
					typeof(SpriteFont),
					typeof(Vector2),
					typeof(Color),
					typeof(float),
					typeof(float),
					typeof(int),
					typeof(int),
					typeof(float),
					typeof(int)
				}
			),
			prefix: new HarmonyMethod(typeof(HarmonyUtility), nameof(MockDrawTextWithShadow))
		);
		DrawTextWithShadowCalls.Clear();
	}
	
	public static void TearDown()
	{
		DrawTextWithShadowCalls.Clear();
	}

	public static List<(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color)>
		DrawTextWithShadowCalls { get; } = [];

	static bool MockDrawTextWithShadow
	(
		SpriteBatch b,
		string text,
		SpriteFont font,
		Vector2 position,
		Color color
	)
	{
		DrawTextWithShadowCalls.Add((b, text, font, position, color));
		return false;
	}
}
