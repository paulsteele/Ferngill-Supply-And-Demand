using fse.core.extensions;
using fse.core.multiplayer;
using fse.core.services;
using StardewModdingAPI;
using StardewValley;

namespace fse.core.handlers
{
	public class MultiplayerHandler : IHandler
	{
		private readonly EconomyService _economyService;
		private readonly IModHelper _helper;
		private readonly IMonitor _monitor;

		public MultiplayerHandler(
			IModHelper helper,
			IMonitor monitor,
			EconomyService economyService
		)
		{
			_helper = helper;
			_monitor = monitor;
			_economyService = economyService;
		}

		public void Register()
		{
			_helper.Events.Multiplayer.ModMessageReceived += (_, e) =>
			{
				if (_helper.IsMultiplayerMessageOfType(EconomyModelMessage.Type, e))
				{
					HandleEconomyModelMessage(e.ReadAs<EconomyModelMessage>());
				}

				if (_helper.IsMultiplayerMessageOfType(RequestEconomyModelMessage.Type, e))
				{
					HandleRequestEconomyModelMessage(e.ReadAs<RequestEconomyModelMessage>());
				}
			};
		}

		private void HandleEconomyModelMessage(EconomyModelMessage message)
		{
			if (Game1.player.IsMainPlayer)
			{
				return;
			}
			
			_economyService.ReceiveEconomy(message.Model);
		}
		
		private void HandleRequestEconomyModelMessage(RequestEconomyModelMessage readAs)
		{
			if (!Game1.player.IsMainPlayer)
			{
				return;
			}
			
			_economyService.SendEconomyMessage();
		}
	}
}