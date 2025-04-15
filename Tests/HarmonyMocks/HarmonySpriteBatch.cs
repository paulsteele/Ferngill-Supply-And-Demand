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
		harmony.Patch(
			AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw),new []
			{
				typeof(Texture2D),
				typeof(Vector2),
				typeof(Rectangle),
				typeof(Color),
				typeof(float),
				typeof(Vector2),
				typeof(float),
				typeof(SpriteEffects),
				typeof(float)
			}),
			prefix: new HarmonyMethod(typeof(HarmonySpriteBatch), nameof(MockDraw))
		);
		harmony.Patch(
			AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw),new []
			{
				typeof(Texture2D),
				typeof(Rectangle),
				typeof(Color),
			}),
			prefix: new HarmonyMethod(typeof(HarmonySpriteBatch), nameof(MockDrawTiny))
		);
		harmony.Patch(
			AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw),new []
			{
				typeof(Texture2D),
				typeof(Rectangle),
				typeof(Rectangle),
				typeof(Color),
			}),
			prefix: new HarmonyMethod(typeof(HarmonySpriteBatch), nameof(MockDrawSmall))
		);

		DrawCalls = new();
	}
	
	public static void TearDown()
	{
		DrawCalls.Clear();
	}
	
	static bool MockConstructor() => false;

	public static Dictionary<SpriteBatch,
		List<(
		Texture2D texture,
		Vector2 position,
		Rectangle? destinationRectangle,
		Rectangle? sourceRectangle,
		Color color,
		float rotation,
		Vector2 origin,
		float scale,
		SpriteEffects effects,
		float layerDepth
		)>> DrawCalls;

	static bool MockDraw(
		SpriteBatch __instance,
		Texture2D texture,
		Vector2 position,
		Rectangle? sourceRectangle,
		Color color,
		float rotation,
		Vector2 origin,
		float scale,
		SpriteEffects effects,
		float layerDepth
	)
	{
		if (!DrawCalls.ContainsKey(__instance))
		{
			DrawCalls.Add(__instance, []);
		}
		
		DrawCalls[__instance].Add((texture, position, null, sourceRectangle, color, rotation, origin, scale, effects, layerDepth));
		
		return false;
	}
	
	static bool MockDrawSmall(
		SpriteBatch __instance,
		Texture2D texture,
		Rectangle destinationRectangle,
		Rectangle? sourceRectangle,
		Color color
	)
	{
		if (!DrawCalls.ContainsKey(__instance))
		{
			DrawCalls.Add(__instance, []);
		}
		
		DrawCalls[__instance].Add((texture, Vector2.Zero, destinationRectangle, sourceRectangle, color, -1, Vector2.Zero, -1, SpriteEffects.None, -1));
		
		return false;
	}
	
	static bool MockDrawTiny(
		SpriteBatch __instance,
		Texture2D texture,
		Rectangle destinationRectangle,
		Color color
	)
	{
		if (!DrawCalls.ContainsKey(__instance))
		{
			DrawCalls.Add(__instance, []);
		}
		
		DrawCalls[__instance].Add((texture, Vector2.Zero, destinationRectangle, null, color, -1, Vector2.Zero, -1, SpriteEffects.None, -1));
		
		return false;
	}
}
