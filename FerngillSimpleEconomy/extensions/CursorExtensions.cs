using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace fse.core.extensions;

public static class CursorExtensions
{
	public static Vector2 GetUiScaledPosition(this ICursorPosition position) => Utility.ModifyCoordinatesForUIScale(position.ScreenPixels);
}