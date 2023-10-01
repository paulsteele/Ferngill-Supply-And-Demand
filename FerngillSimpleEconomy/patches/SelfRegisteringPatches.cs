using fse.core.services;
using HarmonyLib;
using StardewModdingAPI;

namespace fse.core.patches
{
	public abstract class SelfRegisteringPatches
	{
		protected static IModHelper ModHelper;
		protected static IMonitor Monitor;
		protected static EconomyService EconomyService;

		public static void Initialize(IModHelper modHelper, EconomyService economyService, IMonitor monitor)
		{
			ModHelper = modHelper;
			EconomyService = economyService;
			Monitor = monitor;
		}

		public abstract void Register(Harmony harmony);
	}
}