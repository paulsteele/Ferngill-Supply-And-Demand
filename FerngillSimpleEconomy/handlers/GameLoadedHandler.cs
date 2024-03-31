using fse.core.actions;
using fse.core.extensions;
using fse.core.models;
using fse.core.services;
using GenericModConfigMenu;
using MailFrameworkMod.Api;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;

namespace fse.core.handlers
{
	public class GameLoadedHandler : IHandler
	{
		private readonly IModHelper _helper;
		private readonly IMonitor _monitor;
		private readonly IManifest _manifest;
		private readonly EconomyService _economyService;

		public GameLoadedHandler(
			IModHelper helper, 
			IMonitor monitor,
			IManifest manifest,
			EconomyService economyService
			)
		{
			_helper = helper;
			_monitor = monitor;
			_manifest = manifest;
			_economyService = economyService;
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
				name: ()=> _helper.Translation.Get("fse.config.MenuTabOffset"),
				getValue: () => ConfigModel.Instance.MenuTabOffset,
				setValue: val => ConfigModel.Instance.MenuTabOffset = val
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
			
			var resetButton = new OptionsButton(_helper.Translation.Get("fse.config.Reset"), () => { });
			var resetState = false;

			configMenu.AddComplexOption(
				mod: _manifest,
				name: () => _helper.Translation.Get("fse.config.ResetEconomy"),
				draw: (batch, position) =>
				{
					resetButton.bounds = new Rectangle((int)position.X, (int)position.Y, 300, 60);
					resetButton.draw(batch, 0, 0);
					if (_helper.Input.IsDown(SButton.MouseLeft))
					{
						resetState = true;
					}
					else
					{
						if (resetState && resetButton.bounds.Contains(_helper.Input.GetCursorPosition().GetUiScaledPosition()))
						{
							if (_economyService.Loaded)
							{
								_economyService.SetupForNewYear();
								_economyService.AdvanceOneDay();
							}
						}

						resetState = false;
					}
				}
			);
		}
	}
}