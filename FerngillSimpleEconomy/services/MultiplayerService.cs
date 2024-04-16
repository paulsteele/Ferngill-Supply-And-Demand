using System.Linq;
using fse.core.multiplayer;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace fse.core.services;

public interface IMultiplayerService
{
	bool IsMultiplayerMessageOfType(string type, ModMessageReceivedEventArgs e);
	void SendMessageToPeers(IMessage message);
}

public class MultiplayerService(IModHelper helper) : IMultiplayerService
{
	public bool IsMultiplayerMessageOfType(string type, ModMessageReceivedEventArgs e) => e.FromModID == helper.ModContent.ModID && e.Type == type;

	public void SendMessageToPeers(IMessage message)
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