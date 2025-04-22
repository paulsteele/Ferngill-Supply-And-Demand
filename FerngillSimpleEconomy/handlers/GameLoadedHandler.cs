using System.Linq;
using fse.core.actions;
using fse.core.extensions;
using fse.core.integrations;
using fse.core.models;
using fse.core.services;
using GenericModConfigMenu;

using Leclair.Stardew.BetterGameMenu;
using LeFauxMods.Common.Integrations.IconicFramework;
using MailFrameworkMod.Api;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace fse.core.handlers;

public class GameLoadedHandler(
	IModHelper helper,
	IMonitor monitor,
	IManifest manifest,
	IEconomyService economyService,
	IBetterGameMenuService betterGameMenuService,
	IIconicFrameworkService iconicFrameworkService,
	IStarControlService starControlService
) : IHandler
{
	public void Register()
	{
		helper.Events.GameLoop.GameLaunched += (_, _) => SafeAction.Run(OnLaunched, monitor, nameof(OnLaunched));
	}

	private void OnLaunched()
	{
		RegisterBetterGameMenu();
		RegisterIconicAndStarControl();
		RegisterMailFramework();
		RegisterGenericConfig();
	}
	
	private void RegisterIconicAndStarControl()
	{
		var iconicFramework = helper.ModRegistry.GetApi<IIconicFrameworkApi>("furyx639.ToolbarIcons");
		var starControlApi = helper.ModRegistry.GetApi<IStarControlApi>("StarControl.API");
		
		iconicFrameworkService.Register(iconicFramework);
		starControlService.Register(starControlApi, iconicFramework);
	}

	private void RegisterBetterGameMenu()
	{
		var betterGameMenuApi = helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");
		betterGameMenuService.Register(betterGameMenuApi);
	}

	private void RegisterMailFramework()
	{
		var mailFrameworkModApi = helper.ModRegistry.GetApi<IMailFrameworkModApi>("DIGUS.MailFrameworkMod");
		if (mailFrameworkModApi == null)
		{
			return;
		}

		var contentPack = helper.ContentPacks.CreateTemporary($"{helper.DirectoryPath}/assets/mail", $"{helper.ModContent.ModID}.mail", "fsemail", "fsemail", "fse", manifest.Version);
		mailFrameworkModApi.RegisterContentPack(contentPack);
	}

	private void RegisterGenericConfig()
	{
		var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

		// ReSharper disable once UseNullPropagation
		if (configMenu is null)
		{
			return;
		}
			
		configMenu.Register(
			mod: manifest,
			reset: () => ConfigModel.Instance = new ConfigModel(),
			save: () => helper.WriteConfig(ConfigModel.Instance)
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.MaxCalculatedSupply"),
			getValue: () => ConfigModel.Instance.MaxCalculatedSupply,
			setValue: val => ConfigModel.Instance.MaxCalculatedSupply = val,
			min: 0
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.MinDelta"),
			getValue: () => ConfigModel.Instance.MinDelta,
			setValue: val => ConfigModel.Instance.MinDelta = val
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.MaxDelta"),
			getValue: () => ConfigModel.Instance.MaxDelta,
			setValue: val => ConfigModel.Instance.MaxDelta = val
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.DeltaArrow"),
			getValue: () => ConfigModel.Instance.DeltaArrow,
			setValue: val => ConfigModel.Instance.DeltaArrow = val
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.StdDevSupply"),
			getValue: () => ConfigModel.Instance.StdDevSupply,
			setValue: val => ConfigModel.Instance.StdDevSupply = val,
			min: 0
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.StdDevDelta"),
			getValue: () => ConfigModel.Instance.StdDevDelta,
			setValue: val => ConfigModel.Instance.StdDevDelta = val,
			min: 0
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.StdDevDeltaInSeason"),
			getValue: () => ConfigModel.Instance.StdDevDeltaInSeason,
			setValue: val => ConfigModel.Instance.StdDevDeltaInSeason = val,
			min: 0
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.StdDevDeltaOutOfSeason"),
			getValue: () => ConfigModel.Instance.StdDevDeltaOutOfSeason,
			setValue: val => ConfigModel.Instance.StdDevDeltaOutOfSeason = val,
			min: 0
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.MinPercentage"),
			getValue: () => ConfigModel.Instance.MinPercentage,
			setValue: val => ConfigModel.Instance.MinPercentage = val,
			min: 0f
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.MaxPercentage"),
			getValue: () => ConfigModel.Instance.MaxPercentage,
			setValue: val => ConfigModel.Instance.MaxPercentage = val,
			min: 0f
		);
			
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.MenuTabOffset"),
			getValue: () => ConfigModel.Instance.MenuTabOffset,
			setValue: val => ConfigModel.Instance.MenuTabOffset = val
		);
			
		configMenu.AddBoolOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.EnableMenuTab"),
			getValue: () => ConfigModel.Instance.EnableMenuTab,
			setValue: val => ConfigModel.Instance.EnableMenuTab = val
		);
			
		configMenu.AddBoolOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.EnableTooltip"),
			getValue: () => ConfigModel.Instance.EnableTooltip,
			setValue: val => ConfigModel.Instance.EnableTooltip = val
		);
			
		configMenu.AddBoolOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.EnableShopDisplay"),
			getValue: () => ConfigModel.Instance.EnableShopDisplay,
			setValue: val => ConfigModel.Instance.EnableShopDisplay = val
		);
			
		configMenu.AddBoolOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.DisableArtisanMapping"),
			getValue: () => ConfigModel.Instance.DisableArtisanMapping,
			setValue: val => ConfigModel.Instance.DisableArtisanMapping = val
		);
			
		configMenu.AddKeybindList(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.hotkey.openMenu"),
			getValue: () => ConfigModel.Instance.ShowMenuHotkey,
			setValue: val => ConfigModel.Instance.ShowMenuHotkey = val
		);
			
		configMenu.AddSectionTitle(
			mod: manifest,
			text: ()=> helper.Translation.Get("fse.config.category.header")
		);
			
		configMenu.AddParagraph(
			mod: manifest,
			text: ()=> helper.Translation.Get("fse.config.category.subtitle")
		);

		var potentialCategories = Game1.objectData.Keys.Select(k => new Object(k, 1)).DistinctBy(o => o.Category).Select(o => (o.Category, Name: o.getCategoryName()));
			
		foreach (var category in potentialCategories)
		{
			configMenu.AddBoolOption(
				mod: manifest,
				name: ()=> $"{category.Name} ({category.Category})",
				getValue: () => ConfigModel.Instance.ValidCategories.Contains(category.Category),
				setValue: val =>
				{
					if (val)
					{
						if (ConfigModel.Instance.ValidCategories.Contains(category.Category))
						{
							return;
						}
						ConfigModel.Instance.ValidCategories.Add(category.Category);
					}
					else
					{
						ConfigModel.Instance.ValidCategories.Remove(category.Category);
					}
				}
			);
		}
			
		var resetButton = new OptionsButton(helper.Translation.Get("fse.config.Reset"), () => { });
		var resetState = false;

		configMenu.AddComplexOption(
			mod: manifest,
			name: () => helper.Translation.Get("fse.config.ResetEconomy"),
			draw: (batch, position) =>
			{
				resetButton.bounds = new Rectangle((int)position.X, (int)position.Y, 300, 60);
				resetButton.draw(batch, 0, 0);
				if (helper.Input.IsDown(SButton.MouseLeft))
				{
					resetState = true;
				}
				else
				{
					if (resetState && resetButton.bounds.Contains(helper.Input.GetCursorPosition().GetUiScaledPosition()))
					{
						if (Game1.player.IsMainPlayer)
						{
							if (economyService.Loaded)
							{
								economyService.Reset();
								economyService.AdvanceOneDay();
							}
						}
					}

					resetState = false;
				}
			}
		);
	}
}
