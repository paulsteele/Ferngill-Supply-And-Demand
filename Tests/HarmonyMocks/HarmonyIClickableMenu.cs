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
		harmony.Patch(
			AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawTextureBox), new []
			{
				typeof(SpriteBatch),
				typeof(Texture2D),
				typeof(Rectangle),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(Color),
				typeof(float),
				typeof(bool),
				typeof(float),
			}),
			prefix: new HarmonyMethod(typeof(HarmonyIClickableMenu), nameof(MockDrawTextureBox))
		);
		harmony.Patch(
			AccessTools.Method(typeof(IClickableMenu), "drawHorizontalPartition", new []
			{
				typeof(SpriteBatch),
				typeof(int),
				typeof(bool),
				typeof(int),
				typeof(int),
				typeof(int),
			}),
			prefix: new HarmonyMethod(typeof(HarmonyIClickableMenu), nameof(MockDrawHorizontalPartition))
		);
		harmony.Patch(
			AccessTools.Method(typeof(IClickableMenu), "drawVerticalPartition", new []
			{
				typeof(SpriteBatch),
				typeof(int),
				typeof(bool),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int),
			}),
			prefix: new HarmonyMethod(typeof(HarmonyIClickableMenu), nameof(MockDrawVerticalPartition))
		);

		DrawMouseCalls = new();
		DrawHoverTextCalls = new();
		DrawTextureBoxCalls = new();
		DrawHoriztonalPartitionCalls = new();
		DrawVerticalPartitionCalls = new();
	}

	public static Dictionary<SpriteBatch, int> DrawMouseCalls;
	public static Dictionary<SpriteBatch, string> DrawHoverTextCalls;
	public static Dictionary<SpriteBatch, 
		List<(
			Texture2D texture,
			Rectangle sourceRect,
			int x,
			int y,
			int width,
			int height,
			Color color,
			float scale,
			bool drawShadow,
			float draw_layer
		)>> DrawTextureBoxCalls;
	
	public static Dictionary<SpriteBatch, 
		List<(
			int height,
			int yPositionOnScreen,
			int yPosition,
			bool small,
			int red,
			int green,
			int blue
		)>> DrawHoriztonalPartitionCalls;
	
	public static Dictionary<SpriteBatch, 
		List<(
			int height,
			int yPositionOnScreen,
			int xPosition,
			bool small,
			int red,
			int green,
			int blue,
			int heightOverride
		)>> DrawVerticalPartitionCalls;

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
	
	static bool MockDrawTextureBox
	(
		SpriteBatch b,
		Texture2D texture,
		Rectangle sourceRect,
		int x,
		int y,
		int width,
		int height,
		Color color,
		float scale,
		bool drawShadow,
		// ReSharper disable once InconsistentNaming
		float draw_layer
	)
	{
		#pragma warning disable CA1854
		if (!DrawTextureBoxCalls.ContainsKey(b))
		#pragma warning restore CA1854
		{
			DrawTextureBoxCalls.Add(b, []);
		}
		
		DrawTextureBoxCalls[b].Add((texture, sourceRect, x, y, width, height, color, scale, drawShadow, draw_layer));

		return false;
	}
	
	static bool MockDrawHorizontalPartition
	(
		IClickableMenu __instance,
		SpriteBatch b,
		int yPosition,
		bool small,
		int red,
		int green,
		int blue
	)
	{
		#pragma warning disable CA1854
		if (!DrawHoriztonalPartitionCalls.ContainsKey(b))
		#pragma warning restore CA1854
		{
			DrawHoriztonalPartitionCalls.Add(b, []);
		}

		DrawHoriztonalPartitionCalls[b].Add((__instance.height, __instance.yPositionOnScreen, yPosition, small, red, green, blue));

		return false;
	}
	
	static bool MockDrawVerticalPartition
	(
		IClickableMenu __instance,
		SpriteBatch b,
		int xPosition,
		bool small,
		int red,
		int green,
		int blue,
		int heightOverride
	)
	{
		#pragma warning disable CA1854
		if (!DrawVerticalPartitionCalls.ContainsKey(b))
		#pragma warning restore CA1854
		{
			DrawVerticalPartitionCalls.Add(b, []);
		}

		DrawVerticalPartitionCalls[b].Add((__instance.height, __instance.yPositionOnScreen, xPosition, small, red, green, blue, heightOverride));

		return false;
	}
}