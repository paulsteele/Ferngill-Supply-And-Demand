﻿using fse.core.handlers;
using fse.core.helpers;
using fse.core.menu;
using fse.core.models;
using fse.core.patches;
using fse.core.services;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace fse.core;

// ReSharper disable once UnusedType.Global
public class FerngillSimpleEconomy : Mod
{
	public override void Entry(IModHelper helper)
	{
		ConfigModel.Instance = helper.ReadConfig<ConfigModel>();
		var multiplayerService = new MultiplayerService(helper);
		var seedService = new SeedService(Monitor);
		var fishService = new FishService(Monitor);
		var artisanService = new ArtisanService(Monitor, helper);
		var normalDistributionService = new NormalDistributionService();
		var updateFrequencyService = new UpdateFrequencyService();
		var economyService = new EconomyService(helper, Monitor, multiplayerService, fishService, seedService, artisanService, normalDistributionService, updateFrequencyService);
		var drawSupplyBarHelper = new DrawSupplyBarHelper(economyService);
		var forecastMenuService = new ForecastMenuService(helper, economyService, new DrawTextHelper(), drawSupplyBarHelper);
		var betterGameMenuService = new BetterGameMenuService(ModManifest, helper, forecastMenuService);
		var iconicFrameworkService = new IconicFrameworkService(helper, forecastMenuService);
		var starControlService = new StarControlService(ModManifest, helper, forecastMenuService);
		var genericConfigMenuService = new GenericConfigMenuService(helper, ModManifest, economyService);
		var tooltipMenu = new TooltipMenu(helper, economyService, drawSupplyBarHelper, betterGameMenuService);
		RegisterPatches(economyService, drawSupplyBarHelper);
		RegisterHandlers(
			helper, 
			economyService, 
			forecastMenuService, 
			betterGameMenuService, 
			iconicFrameworkService, 
			starControlService, 
			genericConfigMenuService, 
			tooltipMenu, 
			multiplayerService
		);
		helper.ConsoleCommands.Add("fse_reset", "Fully Resets Ferngill Simple Economy", (_, _) =>
		{
			if (!Game1.player.IsMainPlayer)
			{
				return;
			}

			economyService.Reset(true, true, SeasonHelper.GetCurrentSeason());
			economyService.AdvanceOneDay();
		});
		helper.ConsoleCommands.Add("fse_reset_daily", "Resets Daily Supply Change of Ferngill Simple Economy", (_, _) =>
		{
			if (!Game1.player.IsMainPlayer)
			{
				return;
			}

			economyService.Reset(false, true, SeasonHelper.GetCurrentSeason());
			economyService.AdvanceOneDay();
		});
	}

	private void RegisterPatches
	(
		EconomyService economyService, 
		IDrawSupplyBarHelper drawSupplyBarHelper
	)
	{
		var harmony = new Harmony(ModManifest.UniqueID);

		SelfRegisteringPatches.Initialize(economyService, Monitor, drawSupplyBarHelper);
		new ObjectPatches().Register(harmony);
		new ShopMenuPatches().Register(harmony);
	}

	private void RegisterHandlers
	(
		IModHelper helper, 
		EconomyService economyService, 
		ForecastMenuService forecastMenuService, 
		BetterGameMenuService betterGameMenuService,
		IconicFrameworkService iconicFrameworkService,
		StarControlService starControlService,
		GenericConfigMenuService genericConfigMenuService,
		TooltipMenu tooltipMenu, 
		MultiplayerService multiplayerService
	)
	{
		new DayEndHandler(helper, Monitor, economyService).Register();
		new SaveLoadedHandler(helper, Monitor, economyService).Register();
		new GameLoadedHandler(helper, Monitor, ModManifest, betterGameMenuService, iconicFrameworkService, starControlService, genericConfigMenuService).Register();
		new GameMenuLoadedHandler(helper, Monitor, forecastMenuService, tooltipMenu).Register();
		new MultiplayerHandler(helper, economyService, multiplayerService).Register();
		new HotkeyHandler(helper, forecastMenuService).Register();
	}
}
