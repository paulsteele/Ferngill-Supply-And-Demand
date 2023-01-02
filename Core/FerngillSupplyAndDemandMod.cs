using fsd.core.handlers;
using fsd.core.menu;
using fsd.core.patches;
using fsd.core.services;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace fsd.core
{
	// ReSharper disable once UnusedType.Global
	public class FerngillSupplyAndDemandMod : Mod
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
			helper.Events.Input.ButtonPressed += (sender, args) =>
			{
				if (args.Button == SButton.I)
				{
					Game1.activeClickableMenu = new ForecastMenu(_economyService, Monitor);
				}
			};
		}
	}
}