using System.IO;
using System.Threading;
using fsd.core.actions;
using fsd.core.integrations;
using fsd.core.menu;
using fsd.core.services;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace fsd.core.handlers
{
	public class GameLoadedHandler : IHandler
	{
		private readonly EconomyService _economyService;
		private readonly IModHelper _helper;
		private readonly IMonitor _monitor;
		public GameLoadedHandler(EconomyService economyService, IModHelper helper, IMonitor monitor)
		{
			_economyService = economyService;
			_helper = helper;
			_monitor = monitor;
		}

		public void Register()
		{
			_helper.Events.GameLoop.GameLaunched +=
				(_, _) => SafeAction.Run(OnLaunched, _monitor, nameof(OnLaunched));
		}

		private void OnLaunched()
		{
			var api = _helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
			if (api == null)
			{
				_monitor.Log("Could not load phone app. Menu will not be accessible. The rest of the mod will still function", LogLevel.Error);
				return;
			}

			var appIcon = _helper.ModContent.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
			var success = api.AddApp(_helper.ModRegistry.ModID, "Ferngill Economic Forecast", () =>
			{
				Game1.activeClickableMenu = new ForecastMenu(_economyService, _monitor);
			}, appIcon);
			if (success)
			{
				_monitor.Log($"loaded phone app successfully", LogLevel.Debug);
			}
			else
			{
				_monitor.Log("Could not load phone app. Menu will not be accessible. The rest of the mod will still function", LogLevel.Error);
			}
		}
	}
}