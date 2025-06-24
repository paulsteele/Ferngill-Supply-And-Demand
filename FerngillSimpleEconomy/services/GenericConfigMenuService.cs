using System;
using System.Linq;
using fse.core.extensions;
using fse.core.models;
using fse.core.helpers;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace fse.core.services;

public interface IGenericConfigMenuService
{
	/// <summary>
	/// Register the mod configuration with the Generic Mod Config Menu API
	/// </summary>
	void Register(IGenericModConfigMenuApi? configMenu);
}

public class GenericConfigMenuService(
	IModHelper helper,
	IManifest manifest,
	IEconomyService economyService
) : IGenericConfigMenuService
{
	private const string AdvancedPageId = "advanced";
	private const string CategoriesPageId = "categories";
	private const string MenuPageId = "menu";
	private const string FrequencyPageId = "frequency";
	
	public void Register(IGenericModConfigMenuApi? configMenu)
	{
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
			name: ()=> helper.Translation.Get("fse.config.MinPercentage"),
			getValue: () => (float)ConfigModel.Instance.MinPercentage,
			setValue: val => ConfigModel.Instance.MinPercentage = (decimal)val,
			min: 0f
		);

		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.MaxPercentage"),
			getValue: () => (float)ConfigModel.Instance.MaxPercentage,
			setValue: val => ConfigModel.Instance.MaxPercentage = (decimal)val,
			min: 0f
		);

		configMenu.AddPageLink(manifest, FrequencyPageId, () => helper.Translation.Get("fse.config.page.frequency"));
		configMenu.AddPageLink(manifest, MenuPageId, () => helper.Translation.Get("fse.config.page.menu"));
		configMenu.AddPageLink(manifest, CategoriesPageId, () => helper.Translation.Get("fse.config.page.categories"));
		configMenu.AddPageLink(manifest, AdvancedPageId, () => helper.Translation.Get("fse.config.page.advanced"));

		PopulateFrequencyPage(configMenu);
		PopulateMenuPage(configMenu);
		PopulateCategoriesPage(configMenu);
		PopulateAdvancedPage(configMenu);
	}

	private void PopulateAdvancedPage(IGenericModConfigMenuApi configMenu)
	{
		configMenu.AddPage(manifest, "advanced", () => helper.Translation.Get("fse.config.page.advanced"));

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

		configMenu.AddBoolOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.DisableArtisanMapping"),
			getValue: () => ConfigModel.Instance.DisableArtisanMapping,
			setValue: val => ConfigModel.Instance.DisableArtisanMapping = val
		);

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
								economyService.Reset(true, true, SeasonHelper.GetCurrentSeason());
								economyService.AdvanceOneDay();
							}
						}
					}

					resetState = false;
				}
			}
		);
	}
	
	private void PopulateCategoriesPage(IGenericModConfigMenuApi configMenu)
	{
		configMenu.AddPage(manifest, CategoriesPageId, () => helper.Translation.Get("fse.config.page.categories"));

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
	}

	private void PopulateFrequencyPage(IGenericModConfigMenuApi configMenu)
	{
		configMenu.AddPage(manifest, FrequencyPageId, () => helper.Translation.Get("fse.config.page.frequency"));
		
		configMenu.AddSectionTitle(mod: manifest, text: () => helper.Translation.Get("fse.forecast.menu.sort.supply"));
		
		configMenu.AddTextOption(
			mod: manifest,
			name: () => helper.Translation.Get("fse.config.frequency.description"),
			getValue: () => ConfigModel.Instance.SupplyUpdateFrequency.ToString(),
			setValue: val =>
			{
				if (Enum.TryParse<UpdateFrequency>(val, true, out var frequency))
				{
					ConfigModel.Instance.SupplyUpdateFrequency = frequency;
				}
			},
			allowedValues: Enum.GetNames(typeof(UpdateFrequency)),
			formatAllowedValue: val => helper.Translation.Get($"fse.config.frequency.{val}")
		);

		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.frequency.description.custom"),
			getValue: () => ConfigModel.Instance.CustomSupplyUpdateFrequency,
			setValue: val => ConfigModel.Instance.CustomSupplyUpdateFrequency = val,
			min: 1
		);

		configMenu.AddSectionTitle(mod: manifest, text: () => helper.Translation.Get("fse.forecast.menu.sort.delta"));

		configMenu.AddTextOption(
			mod: manifest,
			name: () => helper.Translation.Get("fse.config.frequency.description"),
			getValue: () => ConfigModel.Instance.DeltaUpdateFrequency.ToString(),
			setValue: val =>
			{
				if (Enum.TryParse<UpdateFrequency>(val, true, out var frequency))
				{
					ConfigModel.Instance.DeltaUpdateFrequency = frequency;
				}
			},
			allowedValues: Enum.GetNames(typeof(UpdateFrequency)),
			formatAllowedValue: val => helper.Translation.Get($"fse.config.frequency.{val}")
		);

		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.frequency.description.custom"),
			getValue: () => ConfigModel.Instance.CustomDeltaUpdateFrequency,
			setValue: val => ConfigModel.Instance.CustomDeltaUpdateFrequency = val,
			min: 1
		);
	}

	private void PopulateMenuPage(IGenericModConfigMenuApi configMenu)
	{
		configMenu.AddPage(manifest, MenuPageId, () => helper.Translation.Get("fse.config.page.menu"));
		
		configMenu.AddKeybindList(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.hotkey.openMenu"),
			getValue: () => ConfigModel.Instance.ShowMenuHotkey,
			setValue: val => ConfigModel.Instance.ShowMenuHotkey = val
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
		
		configMenu.AddNumberOption(
			mod: manifest,
			name: ()=> helper.Translation.Get("fse.config.MenuTabOffset"),
			getValue: () => ConfigModel.Instance.MenuTabOffset,
			setValue: val => ConfigModel.Instance.MenuTabOffset = val
		);
	}
}