using System.Linq;
using fse.core.actions;
using fse.core.services;
using StardewModdingAPI;
using StardewValley;

namespace fse.core.handlers;

public class DayEndHandler(
	IModHelper helper,
	IMonitor monitor,
	IEconomyService economyService)
	: IHandler
{

	public void Register()
	{
		helper.Events.GameLoop.DayEnding += (_, _) => 
			SafeAction.Run(GameLoopOnDayEnding, monitor, nameof(GameLoopOnDayEnding));
		helper.Events.GameLoop.DayStarted += (_, _) =>
			SafeAction.Run(economyService.AdvanceOneDay, monitor, nameof(economyService.AdvanceOneDay));
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
				economyService.AdjustSupply(item as Object, item.Stack, false);
			}
		}

		HandleEndOfSeason();
	}

	private void HandleEndOfSeason()
	{
		if (economyService.UpdateSupplyYear)
		{
			economyService.SetupForNewYear();
			return; // Skip seasonal update if yearly is happening.
		}

		if (economyService.UpdateSupplySeason)
		{
			economyService.SetupForNewSeason();
		}
	}
}