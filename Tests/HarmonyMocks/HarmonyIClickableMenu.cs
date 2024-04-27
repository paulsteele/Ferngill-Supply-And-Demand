using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;

public class HarmonyIClickableMenu
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Method(
				typeof(IClickableMenu),
				nameof(IClickableMenu.drawMouse),
				new [] { typeof(SpriteBatch), typeof(bool), typeof(int) }),
			prefix: new HarmonyMethod(typeof(HarmonyIClickableMenu), nameof(MockDrawMouse))
		);
		harmony.Patch(
			AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new []
			{
				typeof(SpriteBatch),
				typeof(string),
				typeof(SpriteFont),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(string),
				typeof(int),
				typeof(string[]),
				typeof(Item),
				typeof(int),
				typeof(string),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(float),
				typeof(CraftingRecipe),
				typeof(IList<Item>),
				typeof(Texture2D),
				typeof(Rectangle?),
				typeof(Color?),
				typeof(Color?),
				typeof(float),
				typeof(int),
				typeof(int),
				
			}),
			prefix: new HarmonyMethod(typeof(HarmonyIClickableMenu), nameof(MockDrawHoverText))
		);

		DrawMouseCalls = new Dictionary<SpriteBatch, int>();
		DrawHoverTextCalls = new Dictionary<SpriteBatch, string>();
	}

	public static Dictionary<SpriteBatch, int> DrawMouseCalls;
	public static Dictionary<SpriteBatch, string> DrawHoverTextCalls;

	static bool MockDrawMouse
	(
		SpriteBatch b
	)
	{
		DrawMouseCalls.TryAdd(b, 0);
		DrawMouseCalls[b] += 1;

		return false;
	}
	
	static bool MockDrawHoverText
	(
		SpriteBatch b,
		string text
	)
	{
		if (!DrawHoverTextCalls.TryAdd(b, text))
		{
			DrawHoverTextCalls[b] = text;
		}

		return false;
	}
}