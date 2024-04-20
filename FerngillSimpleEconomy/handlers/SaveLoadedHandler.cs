using fse.core.actions;
using fse.core.services;
using StardewModdingAPI;

namespace fse.core.handlers;

public class SaveLoadedHandler(
	IModHelper helper,
	IMonitor monitor,
	IEconomyService economyService
) : IHandler
{
	public void Register()
	{
		helper.Events.GameLoop.SaveLoaded +=
			(_, _) => SafeAction.Run(GameLoopOnSaveLoaded, monitor, nameof(GameLoopOnSaveLoaded));
	}

	private void GameLoopOnSaveLoaded()
	{
		economyService.OnLoaded();
	}
}