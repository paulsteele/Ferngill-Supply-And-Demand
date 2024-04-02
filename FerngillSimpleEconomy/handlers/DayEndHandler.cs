using System.Linq;
using fse.core.actions;
using fse.core.services;
using StardewModdingAPI;
using StardewValley;

namespace fse.core.handlers
{
	public class DayEndHandler : IHandler
	{
		private const int LastDayOfMonth = 28;
		private readonly EconomyService _economyService;
		private readonly IModHelper _helper;
		private readonly IMonitor _monitor;

		public DayEndHandler(
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
			_helper.Events.GameLoop.DayEnding += (_, _) => 
				SafeAction.Run(GameLoopOnDayEnding, _monitor, nameof(GameLoopOnDayEnding));
			_helper.Events.GameLoop.DayStarted += (_, _) =>
				SafeAction.Run(_economyService.AdvanceOneDay, _monitor, nameof(_economyService.AdvanceOneDay));
		}

		private void GameLoopOnDayEnding()
		{
			if (!Game1.player.IsMainPlayer)
			{
				return;
			}

			var farmers = Game1.getAllFarmers();

			if (!Game1.player.team.useSeparateWallets.Value)
			{
				farmers = new[] { farmers.First() };
			}
			
			foreach (var farmer in farmers)
			{
				foreach (var item in Game1.getFarm().getShippingBin(farmer).Where(item => item is Object))
				{
					// don't notify as entire economy will be synchronized at the start of the day
					_economyService.AdjustSupply(item as Object, item.Stack, false);
				}
			}

			HandleEndOfSeason();
		}

		private void HandleEndOfSeason()
		{
			if (Game1.dayOfMonth < LastDayOfMonth)
			{
				return;
			}

			if (Utility.getSeasonNumber(Game1.currentSeason) == 3)
			{
				_economyService.SetupForNewYear();
			}
			else
			{
				_economyService.SetupForNewSeason();
			}
		}
	}
}