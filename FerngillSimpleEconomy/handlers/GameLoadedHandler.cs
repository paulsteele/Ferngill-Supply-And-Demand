using fse.core.actions;
using fse.core.services;
using MailFrameworkMod.Api;
using StardewModdingAPI;

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
			RegisterMailFramework();
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