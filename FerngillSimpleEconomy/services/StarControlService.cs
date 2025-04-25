using System;
using LeFauxMods.Common.Integrations.IconicFramework;
using Microsoft.Xna.Framework.Graphics;
using StarControl;
using StardewModdingAPI;
using StardewValley;

namespace fse.core.services;

public interface IStarControlService
{
	void Register(IStarControlApi? api, IIconicFrameworkApi? iconicFrameworkApi);
}

public class StarControlService(
	IManifest manifest,
	IModHelper helper,
	IForecastMenuService forecastMenuService) : IStarControlService
{
	public void Register(IStarControlApi? api, IIconicFrameworkApi? iconicFrameworkApi)
	{
		if (api == null)
		{
			return;
		}

		// Early return if IconicFrameworkApi is available
		if (iconicFrameworkApi != null)
		{
			return;
		}

		api.RegisterItems(manifest, [new StarControlItem(helper, forecastMenuService)]);
	}
	
	private class StarControlItem(IModHelper helper, IForecastMenuService forecastMenuService) : IRadialMenuItem {
		public string Id { get; } = $"{helper.ModContent.ModID}.starmenu";
		public string Title { get; } = helper.Translation.Get("fse.forecast.menu.tab.title");
		public string Description { get; } = helper.Translation.Get("fse.config.hotkey.openMenu");
		public Texture2D? Texture { get; } = helper.ModContent.Load<Texture2D>("assets/stock-menu.png");

		public ItemActivationResult Activate(Farmer who, DelayedActions delayedActions, ItemActivationType activationType = ItemActivationType.Primary)
		{
			Game1.activeClickableMenu ??= forecastMenuService.CreateMenu(null);
			return ItemActivationResult.Custom;
		}
	}
}