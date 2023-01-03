using fse.core.actions;
using fse.core.services;
using StardewModdingAPI;

namespace fse.core.handlers
{
	public class SaveLoadedHandler : IHandler
	{
		private readonly EconomyService _economyService;
		private readonly IModHelper _helper;
		private readonly IMonitor _monitor;

		public SaveLoadedHandler(
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
			_helper.Events.GameLoop.SaveLoaded +=
				(_, _) => SafeAction.Run(GameLoopOnSaveLoaded, _monitor, nameof(GameLoopOnSaveLoaded));
		}

		private void GameLoopOnSaveLoaded()
		{
			_economyService.OnLoaded();
		}
	}
}