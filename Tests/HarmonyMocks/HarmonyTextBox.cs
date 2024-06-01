using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;

public class HarmonyTextBox
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(TextBox), new []
			{
				typeof(Texture2D),
				typeof(Texture2D),
				typeof(SpriteFont),
				typeof(Color)
			}),
			prefix: new HarmonyMethod(typeof(HarmonyTextBox), nameof(MockConstructor))
		);
	}

	
	static bool MockConstructor() => false;
}