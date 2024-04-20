using fse.core.multiplayer;
using fse.core.services;
using StardewModdingAPI;
using StardewValley;

namespace fse.core.handlers
{
	public class MultiplayerHandler(
		IModHelper helper,
		IEconomyService economyService,
		IMultiplayerService multiplayerService
	)
		: IHandler
	{
		public void Register()
		{
			helper.Events.Multiplayer.ModMessageReceived += (_, e) =>
			{
				if (multiplayerService.IsMultiplayerMessageOfType(EconomyModelMessage.StaticType, e))
				{
					HandleEconomyModelMessage(e.ReadAs<EconomyModelMessage>());
				}

				if (multiplayerService.IsMultiplayerMessageOfType(RequestEconomyModelMessage.StaticType, e))
				{
					HandleRequestEconomyModelMessage();
				}

				if (multiplayerService.IsMultiplayerMessageOfType(SupplyAdjustedMessage.StaticType, e))
				{
					HandleSupplyAdjustedMessage(e.ReadAs<SupplyAdjustedMessage>());
				}
			};
		}

		private void HandleEconomyModelMessage(EconomyModelMessage message)
		{
			if (Game1.player.IsMainPlayer)
			{
				return;
			}
			
			economyService.ReceiveEconomy(message.Model);
		}
		
		private void HandleRequestEconomyModelMessage()
		{
			if (!Game1.player.IsMainPlayer)
			{
				return;
			}
			
			economyService.SendEconomyMessage();
		}
		
		private void HandleSupplyAdjustedMessage(SupplyAdjustedMessage message)
		{
			var obj = new Object(message.ObjectId, 1);
			
			economyService.AdjustSupply(obj, message.Amount, false);
		}
	}
}