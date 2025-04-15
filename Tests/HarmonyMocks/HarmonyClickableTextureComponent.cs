using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace Tests.HarmonyMocks;

public class HarmonyClickableTextureComponent
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch
			(
				AccessTools.Method(typeof(ClickableTextureComponent), nameof(ClickableTextureComponent.draw), new []{typeof(SpriteBatch)}),
				prefix: new HarmonyMethod(typeof(HarmonyClickableTextureComponent), nameof(MockDraw))
			);

		DrawCalls = new();
	}
	
	public static void TearDown()
	{
		DrawCalls.Clear();
	}

	public static Dictionary<ClickableTextureComponent, int> DrawCalls;
	
	static bool MockDraw(ClickableTextureComponent __instance)
	{
		DrawCalls.TryAdd(__instance, 0);
		DrawCalls[__instance]++;
		return false;
	}
}
