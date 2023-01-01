using fsd.core.actions;
using fsd.core.services;
using StardewModdingAPI;
using StardewValley;

namespace fsd.core.handlers
{
	public class DayEndHandler : IHandler
	{
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
			_helper.Events.GameLoop.DayEnding +=
				(_, _) => SafeAction.Run(GameLoopOnDayEnding, _monitor, nameof(GameLoopOnDayEnding));
		}

		private void GameLoopOnDayEnding()
		{
			foreach (var farmer in Game1.getAllFarmers())
			{
				foreach (var item in Game1.getFarm().getShippingBin(farmer))
				{
					_monitor.Log($"sold {item.Name} at {item.salePrice()} {item.Stack}x via shipping", LogLevel.Error);
				}
			}

			_economyService.AdvanceOneDay();
		}
	}
}