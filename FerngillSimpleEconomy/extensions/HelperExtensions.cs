using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace fse.core.extensions;

public static class HelperExtensions
{
	public static bool IsMultiplayerMessageOfType(this IModHelper helper, string type, ModMessageReceivedEventArgs e) => e.FromModID == helper.ModContent.ModID && e.Type == type;
}