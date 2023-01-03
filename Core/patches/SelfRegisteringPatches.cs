using fse.core.services;
using HarmonyLib;
using StardewModdingAPI;

namespace fse.core.patches
{
	public abstract class SelfRegisteringPatches
	{
		protected static IMonitor Monitor;
		protected static EconomyService EconomyService;

		public static void Initialize(EconomyService economyService, IMonitor monitor)
		{
			EconomyService = economyService;
			Monitor = monitor;
		}

		public abstract void Register(Harmony harmony);
	}
}