#nullable enable

using fse.core.models;

using Leclair.Stardew.BetterGameMenu;

using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

using StardewValley.Menus;

using System.Linq;

namespace fse.core.services;

public interface IBetterGameMenuService
{
	/// <summary>
	/// Store the <see cref="IBetterGameMenuApi"/> for later use and
	/// register our custom tab, along with setting up any
	/// necessary events.
	/// </summary>
	void Register(IBetterGameMenuApi api);

	/// <summary>
	/// Get the current menu page of the provided game menu, assuming the
	/// provided menu is a game menu. If the menu isn't a game menu, this
	/// returns <c>null</c> instead. This supports both the built-in
	/// <see cref="GameMenu"/> and Better Game Menu.
	/// </summary>
	/// <param name="menu">The menu to get the current page from.</param>
	IClickableMenu? GetCurrentPage(IClickableMenu? menu);

	/// <summary>
	/// This method attempts to change the Better Game Menu of the active
	/// screen back to the previous tab.
	/// </summary>
	void SwitchToLastTab();

}

public class BetterGameMenuService(
	IManifest manifest,
	IModHelper modHelper,
	IForecastMenuService menuService
) : IBetterGameMenuService
{
	private IBetterGameMenuApi? Api;

	private readonly PerScreen<string?> LastTab = new();

	public void Register(IBetterGameMenuApi api)
	{
		Api = api;

		Api.RegisterTab(
			manifest.UniqueID,
			order: (int)VanillaTabOrders.Exit + 10,
			getDisplayName: () => modHelper.Translation.Get("fse.forecast.menu.tab.title"),
			getIcon: () => (Api.CreateDraw(
				modHelper.ModContent.Load<Texture2D>("assets/stock-menu.png"),
				new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16),
				scale: 4f
			), false),
			priority: 0,
			getPageInstance: CreateInstance,
			getDecoration: null,
			getTabVisible: () => ConfigModel.Instance.EnableMenuTab,
			getMenuInvisible: () => true
		);

		Api.OnTabChanged(OnTabChanged);

	}

	private void OnTabChanged(ITabChangedEvent e)
	{
		LastTab.Value = e.OldTab;

		if (e.Tab == manifest.UniqueID && e.Menu.upperRightCloseButton is not null)
		{
			e.Menu.upperRightCloseButton.visible = false;
		}
	}

	public void SwitchToLastTab()
	{
		var menu = Api?.ActiveMenu;
		menu?.TryChangeTab(LastTab.Value ?? menu.VisibleTabs.FirstOrDefault() ?? nameof(VanillaTabOrders.Exit));
	}

	private IClickableMenu CreateInstance(IClickableMenu menu)
	{
		var result = menuService.CreateMenu();
		result.SetBetterGameMenu(this);
		return result;
	}

	public IClickableMenu? GetCurrentPage(IClickableMenu? menu)
	{
		if (menu is GameMenu gameMenu)
		{
			return gameMenu.GetCurrentPage();
		}
		else if (Api is not null && menu is not null)
		{
			return Api.GetCurrentPage(menu);
		}
		return null;
	}

}
