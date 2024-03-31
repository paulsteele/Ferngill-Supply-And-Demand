using fse.core.actions;
using fse.core.menu;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace fse.core.handlers;

public class GameMenuLoadedHandler : IHandler
{
	private readonly IModHelper _helper;
	private readonly IMonitor _monitor;
	private readonly EconomyService _economyService;
	private Texture2D _menuTexture;
	private ClickableComponent _tab;

	public GameMenuLoadedHandler(
		IModHelper helper,
		IMonitor monitor,
		EconomyService economyService
	)
	{
		_helper = helper;
		_monitor = monitor;
		_economyService = economyService;
			
		_tab = new ClickableComponent(
			new Rectangle(0, 0, 64, 64), 
			"forecast", 
			_helper.Translation.Get("fse.forecast.menu.tab.title")
		);
	}

	public void Register()
	{
		_helper.Events.Display.RenderedActiveMenu += (_, args) => SafeAction.Run(() => DrawTab(args.SpriteBatch), _monitor, nameof(DrawTab));
		_helper.Events.Input.ButtonPressed += (_, args) => SafeAction.Run(() => HandleLeftClick(args), _monitor, nameof(HandleLeftClick));
	}

	public void HandleLeftClick(ButtonPressedEventArgs buttonPressedEventArgs)
	{
		if (Game1.activeClickableMenu is not GameMenu gameMenu)
		{
			return;
		}
		
		if (!ConfigModel.Instance.EnableMenuTab)
		{
			return;
		}

		if (buttonPressedEventArgs.Button != SButton.MouseLeft)
		{
			return;
		}
		
		if (!_tab.bounds.Contains(Mouse.GetState().Position))
		{
			return;
		}

		if (gameMenu.pages[gameMenu.currentTab] is ForecastMenu)
		{
			return;
		}

		new ForecastMenu(_helper, _economyService, _monitor).TakeOverMenuTab(gameMenu);
	}

	public void DrawTab(SpriteBatch batch)
	{
		if (!ConfigModel.Instance.EnableMenuTab)
		{
			return;
		}
			
		if (Game1.activeClickableMenu is not GameMenu gameMenu)
		{
			return;
		}
		
		switch (gameMenu.pages[gameMenu.currentTab])
		{
			case MapPage:
			case CollectionsPage { letterviewerSubMenu: not null }:
				return;
		}

		_menuTexture ??= _helper.ModContent.Load<Texture2D>("assets/stock-menu.png");

		_tab.bounds = new Rectangle(
			gameMenu.xPositionOnScreen + (64 * 11) + ConfigModel.Instance.MenuTabOffset,
			gameMenu.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64,
			64,
			64
		);
			
		batch.Draw(
			_menuTexture,
			new Vector2(_tab.bounds.X, _tab.bounds.Y), 
			new Rectangle?(new Rectangle(0 * 16, 0, 16, 16)),
			Color.White, 
			0.0f, 
			Vector2.Zero, 
			4f, 
			SpriteEffects.None, 
			0.0001f
		);
			
		gameMenu.drawMouse(batch);

		var hoverText = gameMenu.hoverText;

		if (_tab.bounds.Contains(Mouse.GetState().Position))
		{
			hoverText = _tab.label;
		}

		if (!string.IsNullOrWhiteSpace(hoverText))
		{
			IClickableMenu.drawHoverText(batch, hoverText, Game1.smallFont);
		}
	}
}