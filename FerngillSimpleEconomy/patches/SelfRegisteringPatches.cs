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

		public static void Initialize(IModHelper modHelper, IEconomyService economyService, IMonitor monitor)
		{
			ModHelper = modHelper;
			EconomyService = economyService;
			Monitor = monitor;
		}

		public abstract void Register(Harmony harmony);
	}
}