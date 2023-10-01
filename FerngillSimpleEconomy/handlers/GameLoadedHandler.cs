using System.IO;
using fse.core.actions;
using fse.core.integrations;
using fse.core.menu;
using fse.core.services;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace fse.core.handlers
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
			var success = api.AddApp(_helper.ModRegistry.ModID, _helper.Translation.Get("fes.appname"), () =>
			{
				Game1.activeClickableMenu = new ForecastMenu(_helper, _economyService, _monitor);
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