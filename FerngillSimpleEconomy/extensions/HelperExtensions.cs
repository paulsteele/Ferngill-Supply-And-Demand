using System.Linq;
using fse.core.multiplayer;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace fse.core.extensions;

public static class HelperExtensions
{
	public static bool IsMultiplayerMessageOfType(this IModHelper helper, string type, ModMessageReceivedEventArgs e) => e.FromModID == helper.ModContent.ModID && e.Type == type;

	public static void SendMessageToPeers(this IModHelper helper, IMessage message)
	{
		var players = Game1.getOnlineFarmers()
			.Where(p => Game1.player != p)
			.Select(p => p.UniqueMultiplayerID)
			.ToArray();
		
		if (!players.Any())
		{
			return;
		}
		
		helper.Multiplayer.SendMessage(message, message.Type, new [] {helper.ModContent.ModID},  players.ToArray());
	}
}