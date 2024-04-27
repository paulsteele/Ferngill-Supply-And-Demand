using fse.core.services;
using HarmonyLib;
using StardewModdingAPI;

namespace fse.core.patches
{
	public abstract class SelfRegisteringPatches
	{
		protected static IModHelper ModHelper;
		protected static IMonitor Monitor;
		protected static IEconomyService EconomyService;
		protected static IForecastMenuService ForecastMenuService;

		public static void Initialize(
			IModHelper modHelper, 
			IEconomyService economyService, 
			IMonitor monitor,
			IForecastMenuService forecastMenuService
		)
		{
			ModHelper = modHelper;
			EconomyService = economyService;
			Monitor = monitor;
			ForecastMenuService = forecastMenuService;
		}

		public abstract void Register(Harmony harmony);
	}
}