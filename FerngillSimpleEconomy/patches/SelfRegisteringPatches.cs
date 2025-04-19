using System;
using fse.core.services;
using HarmonyLib;
using StardewModdingAPI;

namespace fse.core.patches
{
	public abstract class SelfRegisteringPatches
	{
		private static IModHelper? _modHelper;
		protected static IModHelper ModHelper => _modHelper ?? throw new Exception("ModHelper not initialized");
		
		private static IMonitor? _monitor;
		protected static IMonitor Monitor => _monitor ?? throw new Exception("Monitor not initialized");
		
		private static IEconomyService? _economyService;
		protected static IEconomyService EconomyService => _economyService ?? throw new Exception("EconomyService not initialized");
		
		private static IForecastMenuService? _forecastMenuService;
		protected static IForecastMenuService ForecastMenuService => _forecastMenuService ?? throw new Exception("ForecastMenuService not initialized");

		public static void Initialize(
			IModHelper modHelper, 
			IEconomyService economyService, 
			IMonitor monitor,
			IForecastMenuService forecastMenuService
		)
		{
			_modHelper = modHelper;
			_economyService = economyService;
			_monitor = monitor;
			_forecastMenuService = forecastMenuService;
		}

		public abstract void Register(Harmony harmony);
	}
}