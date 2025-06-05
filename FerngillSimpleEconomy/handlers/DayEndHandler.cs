using System.Linq;
using fse.core.actions;
using fse.core.helpers;
using fse.core.models;
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
	private const int LastDayOfMonth = 28;

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
		HandleEndOfDayDynamics();
	}

	private void HandleEndOfDayDynamics() {
		bool isSupplyChange = (Game1.dayOfMonth + (Utility.getSeasonNumber(Game1.currentSeason)*28)) % ConfigModel.Instance.DaysToSupplyChange == 0;
		bool isDeltaChange = Game1.dayOfMonth % ConfigModel.Instance.DaysToDeltaChange == 0;
		if (isSupplyChange || isDeltaChange)
		{
			bool isEndOfMonth = Game1.dayOfMonth == 28;
			economyService.Reset(isSupplyChange, isDeltaChange, (isEndOfMonth)? SeasonHelper.GetNextSeason() : SeasonHelper.GetCurrentSeason());
		}
	}
}
