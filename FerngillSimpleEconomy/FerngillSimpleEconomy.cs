using fse.core.handlers;
using fse.core.helpers;
using fse.core.models;
using fse.core.patches;
using fse.core.services;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace fse.core
{
	// ReSharper disable once UnusedType.Global
	public class FerngillSimpleEconomy : Mod
	{
		private MultiplayerService _multiplayerService;
		private EconomyService _economyService;
		private ForecastMenuService _forecastMenuService;

		public override void Entry(IModHelper helper)
		{
			ConfigModel.Instance = helper.ReadConfig<ConfigModel>();
			_multiplayerService = new MultiplayerService(helper);
			_economyService = new EconomyService(helper, Monitor, _multiplayerService);
			_forecastMenuService = new ForecastMenuService(helper, _economyService, Monitor, new DrawTextHelper());
			RegisterPatches(helper);
			RegisterHandlers(helper);
			helper.ConsoleCommands.Add("fse_reset", "Fully Resets Ferngill Simple Economy", (_, _) =>
			{
				if (!Game1.player.IsMainPlayer)
				{
					return;
				}

				_economyService.SetupForNewYear();
				_economyService.AdvanceOneDay();
			});
		}

		private void RegisterPatches(IModHelper helper)
		{
			var harmony = new Harmony(ModManifest.UniqueID);

			SelfRegisteringPatches.Initialize(helper, _economyService, Monitor, _forecastMenuService);
			new ObjectPatches().Register(harmony);
			new ShopMenuPatches().Register(harmony);
		}

		private void RegisterHandlers(IModHelper helper)
		{
			new DayEndHandler(helper, Monitor, _economyService).Register();
			new SaveLoadedHandler(helper, Monitor, _economyService).Register();
			new GameLoadedHandler(helper, Monitor, ModManifest, _economyService).Register();
			new GameMenuLoadedHandler(helper, Monitor, _forecastMenuService).Register();
			new MultiplayerHandler(helper, _economyService, _multiplayerService).Register();
		}
	}
}