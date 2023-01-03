using fse.core.handlers;
using fse.core.patches;
using fse.core.services;
using HarmonyLib;
using StardewModdingAPI;

namespace fse.core
{
	// ReSharper disable once UnusedType.Global
	public class FerngillSimpleEconomy : Mod
	{
		private EconomyService _economyService;

		public override void Entry(IModHelper helper)
		{
			_economyService = new EconomyService(helper, Monitor);
			RegisterPatches();
			RegisterHandlers(helper);
		}

		private void RegisterPatches()
		{
			var harmony = new Harmony(ModManifest.UniqueID);

			SelfRegisteringPatches.Initialize(_economyService, Monitor);
			new ObjectPatches().Register(harmony);
			new ShopMenuPatches().Register(harmony);
		}

		private void RegisterHandlers(IModHelper helper)
		{
			new DayEndHandler(helper, Monitor, _economyService).Register();
			new SaveLoadedHandler(helper, Monitor, _economyService).Register();
			new GameLoadedHandler(_economyService, helper, Monitor).Register();
		}
	}
}