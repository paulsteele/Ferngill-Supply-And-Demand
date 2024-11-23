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
		
		harmony.Patch(
			original: AccessTools.PropertySetter(typeof(TextBox), nameof(TextBox.Selected)),
			prefix: new HarmonyMethod(typeof(HarmonyTextBox), nameof(MockSelected))
		);

		harmony.Patch(
			original: AccessTools.PropertyGetter(typeof(TextBox), nameof(TextBox.Selected)), 
			prefix: new HarmonyMethod(typeof(HarmonyTextBox), nameof(MockSelected))
		);

		harmony.Patch(
			original: AccessTools.Method(typeof(TextBox), nameof(TextBox.SelectMe)),
			prefix: new HarmonyMethod(typeof(HarmonyTextBox), nameof(MockSelectMe))
		);

		harmony.Patch(
			original: AccessTools.Method(typeof(TextBox), nameof(TextBox.Update)),
			prefix: new HarmonyMethod(typeof(HarmonyTextBox), nameof(MockUpdate))
		);
	}

	static bool MockConstructor() => false;

	private static bool MockSelected() => false;
	private static bool MockSelectMe() => false;
	private static bool MockUpdate() => false;
}