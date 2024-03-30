using fse.core.actions;
using fse.core.models;
using fse.core.services;
using GenericModConfigMenu;
using MailFrameworkMod.Api;
using StardewModdingAPI;

namespace fse.core.handlers
{
	public class GameLoadedHandler : IHandler
	{
		private readonly IModHelper _helper;
		private readonly IMonitor _monitor;
		private readonly IManifest _manifest;

		public GameLoadedHandler(
			IModHelper helper, 
			IMonitor monitor,
			IManifest manifest
			)
		{
			_helper = helper;
			_monitor = monitor;
			_manifest = manifest;
		}

		public void Register()
		{
			_helper.Events.GameLoop.GameLaunched +=
				(_, _) => SafeAction.Run(OnLaunched, _monitor, nameof(OnLaunched));
		}

		private void OnLaunched()
		{
			RegisterMailFramework();
			RegisterGenericConfig();
		}
		
		private void RegisterMailFramework()
		{
			var mailFrameworkModApi = _helper.ModRegistry.GetApi<IMailFrameworkModApi>("DIGUS.MailFrameworkMod");
			if (mailFrameworkModApi == null)
			{
				return;
			}
			
			var contentPack = _helper.ContentPacks.CreateTemporary($"{_helper.DirectoryPath}/assets/mail", $"{_helper.ModContent.ModID}.mail", "fsemail", "fsemail", "fse", _manifest.Version);
			mailFrameworkModApi.RegisterContentPack(contentPack);
		}

		private void RegisterGenericConfig()
		{
			var configMenu = _helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			
			// ReSharper disable once UseNullPropagation
			if (configMenu is null)
			{
				return;
			}
			
			configMenu.Register(
				mod: _manifest,
				reset: () => ConfigModel.Instance = new ConfigModel(),
				save: () => _helper.WriteConfig(ConfigModel.Instance)
			);
			
			configMenu.AddNumberOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.MaxCalculatedSupply"),
				getValue: () => ConfigModel.Instance.MaxCalculatedSupply,
				setValue: val => ConfigModel.Instance.MaxCalculatedSupply = val,
				min: 0
			);
			
			configMenu.AddNumberOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.MinDelta"),
				getValue: () => ConfigModel.Instance.MinDelta,
				setValue: val => ConfigModel.Instance.MinDelta = val
			);
			
			configMenu.AddNumberOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.MaxDelta"),
				getValue: () => ConfigModel.Instance.MaxDelta,
				setValue: val => ConfigModel.Instance.MaxDelta = val
			);
			
			configMenu.AddNumberOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.StdDevSupply"),
				getValue: () => ConfigModel.Instance.StdDevSupply,
				setValue: val => ConfigModel.Instance.StdDevSupply = val,
				min: 0
			);
			
			configMenu.AddNumberOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.StdDevDelta"),
				getValue: () => ConfigModel.Instance.StdDevDelta,
				setValue: val => ConfigModel.Instance.StdDevDelta = val,
				min: 0
			);
			
			configMenu.AddNumberOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.MinPercentage"),
				getValue: () => ConfigModel.Instance.MinPercentage,
				setValue: val => ConfigModel.Instance.MinPercentage = val,
				min: 0f
			);
			
			configMenu.AddNumberOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.MaxPercentage"),
				getValue: () => ConfigModel.Instance.MaxPercentage,
				setValue: val => ConfigModel.Instance.MaxPercentage = val,
				min: 0f
			);
			
			configMenu.AddNumberOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.MenuTabIndex"),
				getValue: () => ConfigModel.Instance.MenuTabIndex,
				setValue: val => ConfigModel.Instance.MenuTabIndex = val,
				min: 0
			);
			
			configMenu.AddBoolOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.EnableMenuTab"),
				getValue: () => ConfigModel.Instance.EnableMenuTab,
				setValue: val => ConfigModel.Instance.EnableMenuTab = val
			);
			
			configMenu.AddBoolOption(
				mod: _manifest,
				name: ()=> _helper.Translation.Get("fse.config.EnableShopDisplay"),
				getValue: () => ConfigModel.Instance.EnableShopDisplay,
				setValue: val => ConfigModel.Instance.EnableShopDisplay = val
			);
		}
	}
}