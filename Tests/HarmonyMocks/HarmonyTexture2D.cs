using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tests.HarmonyMocks;

public class HarmonyTexture2D
{
	public static void Setup(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Constructor(typeof(Texture2D), new []
			{
				typeof(GraphicsDevice), 
				typeof(int),
				typeof(int),
			}),
			prefix: new HarmonyMethod(typeof(HarmonyTexture2D), nameof(MockConstructor))
		);

		harmony.Patch(
			typeof(Texture2D).GetMethods()
				.First(m => m.Name == nameof(Texture2D.SetData) && m.GetParameters().Length == 1)
				.MakeGenericMethod(typeof(Color)),
			prefix: new HarmonyMethod(typeof(HarmonyTexture2D), nameof(MockConstructor))
		);
	}

	static bool MockConstructor() => false;
}