using System.IO;
using fse.core.actions;
using fse.core.integrations;
using fse.core.menu;
using fse.core.services;
using MailFrameworkMod.Api;
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
		private readonly ISemanticVersion _semanticVersion;

		public GameLoadedHandler(
			EconomyService economyService, 
			IModHelper helper, 
			IMonitor monitor,
			ISemanticVersion semanticVersion
			)
		{
			_economyService = economyService;
			_helper = helper;
			_monitor = monitor;
			_semanticVersion = semanticVersion;
		}

		public void Register()
		{
			_helper.Events.GameLoop.GameLaunched +=
				(_, _) => SafeAction.Run(OnLaunched, _monitor, nameof(OnLaunched));
		}

		private void OnLaunched()
		{
			RegisterMobilePhone();
			RegisterMailFramework();
		}

		private void RegisterMobilePhone()
		{
			var api = _helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
			if (api == null)
			{
				_monitor.Log("Could not load phone app. Menu will not be accessible. The rest of the mod will still function", LogLevel.Error);
				return;
			}

			var appIcon = _helper.ModContent.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
			var success = api.AddApp(_helper.ModRegistry.ModID, _helper.Translation.Get("fse.appname"), () =>
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
		
		private void RegisterMailFramework()
		{
			var mailFrameworkModApi = _helper.ModRegistry.GetApi<IMailFrameworkModApi>("DIGUS.MailFrameworkMod");
			if (mailFrameworkModApi == null)
			{
				return;
			}
			
			var contentPack = _helper.ContentPacks.CreateTemporary($"{_helper.DirectoryPath}/assets/mail", $"{_helper.ModContent.ModID}.mail", "fsemail", "fsemail", "fse", _semanticVersion);
			mailFrameworkModApi.RegisterContentPack(contentPack);
		}
	}
}