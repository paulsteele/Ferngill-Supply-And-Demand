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
	void Register(IBetterGameMenuApi? api);

	/// <summary>
	/// Get the current menu page of the provided game menu, assuming the
	/// provided menu is a game menu. If the menu isn't a game menu, this
	/// returns <c>null</c> instead. This supports both the built-in
	/// <see cref="GameMenu"/> and Better Game Menu.
	/// </summary>
	/// <param name="menu">The menu to get the current page from.</param>
	IClickableMenu? GetCurrentPage(IClickableMenu? menu);
}

public class BetterGameMenuService(
	IManifest manifest,
	IModHelper modHelper,
	IForecastMenuService menuService
) : IBetterGameMenuService
{
	private IBetterGameMenuApi? _api;

	private readonly PerScreen<string?> _lastTab = new();

	public void Register(IBetterGameMenuApi? api)
	{
		if (api == null)
		{
			return;
		}
		
		_api = api;

		_api.RegisterTab(
			manifest.UniqueID,
			order: (int)VanillaTabOrders.Exit + 10,
			getDisplayName: () => modHelper.Translation.Get("fse.forecast.menu.tab.title"),
			getIcon: () => (_api.CreateDraw(
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

		_api.OnTabChanged(OnTabChanged);
	}

	private void OnTabChanged(ITabChangedEvent e)
	{
		_lastTab.Value = e.OldTab;

		if (e.Tab == manifest.UniqueID && e.Menu.upperRightCloseButton is not null)
		{
			e.Menu.upperRightCloseButton.visible = false;
		}
	}

	private void SwitchToLastTab()
	{
		var menu = _api?.ActiveMenu;
		menu?.TryChangeTab(_lastTab.Value ?? menu.VisibleTabs.FirstOrDefault() ?? nameof(VanillaTabOrders.Exit));
	}

	private IClickableMenu CreateInstance(IClickableMenu menu) => menuService.CreateMenu(SwitchToLastTab);

	public IClickableMenu? GetCurrentPage(IClickableMenu? menu)
	{
		if (menu is GameMenu gameMenu)
		{
			return gameMenu.GetCurrentPage();
		}

		if (_api is not null && menu is not null)
		{
			return _api.GetCurrentPage(menu);
		}
		
		return null;
	}
}
