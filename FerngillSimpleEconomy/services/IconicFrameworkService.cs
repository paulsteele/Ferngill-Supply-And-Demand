using LeFauxMods.Common.Integrations.IconicFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace fse.core.services;

public interface IIconicFrameworkService
{
	void Register(IIconicFrameworkApi? api);
}

public class IconicFrameworkService(IModHelper helper, IForecastMenuService forecastMenuService) : IIconicFrameworkService
{
	private const string FseStockMenuPng = "fse/stock-menu.png";
	
	public void Register(IIconicFrameworkApi? api)
	{
		if (api == null)
		{
			return;
		}

		helper.Events.Content.AssetRequested += OnContentOnAssetRequested;
		
		api.AddToolbarIcon(
			"fse.forecast", 
			FseStockMenuPng, 
			new Rectangle(0, 0, 16, 16), 
			() => helper.Translation.Get("fse.forecast.menu.tab.title"), 
			() => helper.Translation.Get("fse.config.hotkey.openMenu"),
			() => { Game1.activeClickableMenu ??= forecastMenuService.CreateMenu(null); }
		);
		
		helper.Events.Content.AssetRequested -= OnContentOnAssetRequested;
	}

	private static void OnContentOnAssetRequested(object? _, AssetRequestedEventArgs args)
	{
		if (args.Name.Name == FseStockMenuPng)
		{
			args.LoadFromModFile<Texture2D>("assets/stock-menu.png", AssetLoadPriority.Exclusive);
		}
	}
}