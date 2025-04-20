using fse.core.actions;
using fse.core.extensions;
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
	private readonly IForecastMenuService _forecastMenuService;
	private readonly ITooltipMenu _tooltipMenu;
	private Texture2D? _menuTexture;
	public readonly ClickableComponent Tab;

	public GameMenuLoadedHandler(
		IModHelper helper,
		IMonitor monitor,
		IForecastMenuService forecastMenuService,
		ITooltipMenu tooltipMenu
	)
	{
		_helper = helper;
		_monitor = monitor;
		_forecastMenuService = forecastMenuService;
		_tooltipMenu = tooltipMenu;

		Tab = new ClickableComponent(
			new Rectangle(0, 0, 64, 64), 
			"forecast", 
			_helper.Translation.Get("fse.forecast.menu.tab.title")
		);
	}

	public void Register()
	{
		_helper.Events.Display.RenderedActiveMenu += (_, args) => SafeAction.Run(() => DrawTab(args.SpriteBatch), _monitor, nameof(DrawTab));
		_helper.Events.Input.ButtonPressed += (_, args) => SafeAction.Run(() => HandleButtonPressed(args), _monitor, nameof(HandleButtonPressed));
		_helper.Events.Display.RenderedHud += (_, args) => SafeAction.Run(() => _tooltipMenu.PostRenderHud(args), _monitor, nameof(_tooltipMenu.PostRenderHud));
		_helper.Events.Display.RenderedActiveMenu += (_, args) => SafeAction.Run(() => _tooltipMenu.PostRenderGui(args), _monitor, nameof(_tooltipMenu.PostRenderHud));
	}

	public void HandleButtonPressed(ButtonPressedEventArgs buttonPressedEventArgs)
	{
		if (Game1.activeClickableMenu is not GameMenu gameMenu)
		{
			return;
		}
		
		if (!ConfigModel.Instance.EnableMenuTab)
		{
			return;
		}
		
		if (gameMenu.pages[gameMenu.currentTab] is ForecastMenu)
		{
			return;
		}
		
		// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
		switch (buttonPressedEventArgs.Button)
		{
			case SButton.ControllerA:
			case SButton.MouseLeft:
			{
				if (!Tab.bounds.Contains(buttonPressedEventArgs.Cursor.GetUiScaledPosition()))
				{
					return;
				}

				break;
			}
			case SButton.RightTrigger:
			{
				if (gameMenu.currentTab != gameMenu.pages.Count - 1)
				{
					return;
				}

				break;
			}
			// TODO: these do not work
			case SButton.LeftThumbstickRight:
			case SButton.DPadRight:
			{
				if (gameMenu.tabs[^1].bounds.Contains(buttonPressedEventArgs.Cursor.GetUiScaledPosition()))
				{
					var element = Tab;
					Mouse.SetPosition(element.bounds.X + element.bounds.Width / 2, element.bounds.Y + element.bounds.Height / 2);
				}

				return;
			}
			case SButton.LeftThumbstickLeft:
			case SButton.DPadLeft:
			{
				if (Tab.bounds.Contains(buttonPressedEventArgs.Cursor.GetUiScaledPosition()))
				{
					var element = gameMenu.tabs[^1];
					Mouse.SetPosition(element.bounds.X + element.bounds.Width / 2, element.bounds.Y + element.bounds.Height / 2);
				}

				return;
			}
			default:
				return;
		}

		var forecastMenu = _forecastMenuService.CreateMenu(() => ReleaseMenuTab(gameMenu));
		TakeOverMenuTab(gameMenu, forecastMenu);
	}

	private static void TakeOverMenuTab(GameMenu gameMenu, IForecastMenu forecastMenu)
	{
		Game1.activeClickableMenu = forecastMenu;
			
		gameMenu.invisible = true;
		gameMenu.upperRightCloseButton.visible = false;
	}

	private static void ReleaseMenuTab(GameMenu originalMenu)
	{
		originalMenu.invisible = false;
		originalMenu.upperRightCloseButton.visible = true;
	
		Game1.activeClickableMenu = originalMenu;
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

		if (gameMenu.GetChildMenu() != null)
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
		var offset = ConfigModel.Instance.MenuTabOffset;
		
		if (gameMenu.pages[^1] is not ExitPage)
		{
			offset += 70;
		}

		Tab.bounds = new Rectangle(
			gameMenu.xPositionOnScreen + (64 * 11) + offset,
			gameMenu.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64,
			64,
			64
		);
			
		batch.Draw(
			_menuTexture,
			new Vector2(Tab.bounds.X, Tab.bounds.Y), 
			new Rectangle(0 * 16, 0, 16, 16),
			Color.White, 
			0.0f, 
			Vector2.Zero, 
			4f, 
			SpriteEffects.None, 
			0.0001f
		);
			
		gameMenu.drawMouse(batch);

		var hoverText = gameMenu.hoverText;

		if (Tab.bounds.Contains(_helper.Input.GetCursorPosition().GetUiScaledPosition()))
		{
			hoverText = Tab.label;
		}

		if (!string.IsNullOrWhiteSpace(hoverText))
		{
			IClickableMenu.drawHoverText(batch, hoverText, Game1.smallFont);
		}
	}
}