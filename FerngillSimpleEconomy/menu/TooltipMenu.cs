using System.Collections.Generic;
using System.Linq;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace fse.core.menu
{
	public interface ITooltipMenu
	{
		void PreRenderHud(RenderingHudEventArgs e);
		void PostRenderHud(RenderedHudEventArgs e);
		void PostRenderGui(RenderedActiveMenuEventArgs e);
	}

	public class TooltipMenu : ITooltipMenu
	{
	private static IModHelper _helper;
	private static AbstractForecastMenu _forecastMenu;
	private static IEconomyService _econService;
	private Item _toolbarItem;
	private readonly bool _isUiInfoSuiteLoaded;

	//Constructor
	public TooltipMenu(IModHelper helper, IEconomyService econService, IForecastMenuService forecastMenuService)
	{
		_helper = helper;
	 _econService = econService;

	 _forecastMenu = forecastMenuService.CreateMenu();

	 _isUiInfoSuiteLoaded = _helper.ModRegistry.IsLoaded("Annosz.UiInfoSuite2"); 
	}

	//Get hovered toolbar item
	public void PreRenderHud(RenderingHudEventArgs e)
	{
		if (!ConfigModel.Instance.EnableTooltip)
		{
			return;
		}
		_toolbarItem = null;
		foreach (var menu in Game1.onScreenMenus)
		{
			if (menu is not Toolbar)
			{
				continue;
			}

			_toolbarItem = _helper.Reflection.GetField<Item>(menu, "hoverItem").GetValue();
			return;
		}
	}

	//Render for toolbar item
	public void PostRenderHud(RenderedHudEventArgs e)
	{
		if (!ConfigModel.Instance.EnableTooltip)
		{
			return;
		}
		if (Game1.activeClickableMenu != null || _toolbarItem == null)
		{
			return;
		}

		PopulateHoverTextBoxAndDraw(_toolbarItem);
		_toolbarItem = null;
	}

	//Render for menu item
	public void PostRenderGui(RenderedActiveMenuEventArgs e)
	{
		if (!ConfigModel.Instance.EnableTooltip)
		{
			return;
		}
		if (Game1.activeClickableMenu == null)
		{
			return;
		}

		var item = GetHoveredItemFromMenu(Game1.activeClickableMenu);
		if (item != null)
		{
			PopulateHoverTextBoxAndDraw(item);
		}
	}

	//Get hovered menu item
	private static Item GetHoveredItemFromMenu(IClickableMenu menu)
	{
		switch (menu)
		{
			// game menu
			case GameMenu gameMenu:
			{
				var page = _helper.Reflection.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue()[gameMenu.currentTab];
				if (page is InventoryPage)
				{
					return _helper.Reflection.GetField<Item>(page, "hoveredItem").GetValue();
				}

				break;
			}
			// from inventory UI (so things like shops and so on)
			case MenuWithInventory inventoryMenu:
				return inventoryMenu.hoveredItem;
		}

		return null;
	}

	//Populate and draw
	private void PopulateHoverTextBoxAndDraw(Item item)
	{
		if (item is not Object obj)
		{
			return;
		}
		var model = _econService.GetItemModelFromObject(obj);

		if (model == null)
		{
			return;
		}

		DrawHoverTextBox(model);
	}

	//Draw
	private void DrawHoverTextBox(ItemModel model)
	{
	 const int width = 240;
	 const int height = 110;

	 var x = (int)(Mouse.GetState().X / Game1.options.uiScale) - Game1.tileSize / 2 - width;
	 var y = (int)(Mouse.GetState().Y / Game1.options.uiScale) + Game1.tileSize / 3;

	 //So that the tooltips don't overlap
	 if ((_isUiInfoSuiteLoaded))
	 {
		 x -= 140;
	 }

	 if (x < 0)
	 {
		 x = 0;
	 }

	 if (y + height > Game1.graphics.GraphicsDevice.Viewport.Height)
	 {
		 y = Game1.graphics.GraphicsDevice.Viewport.Height - height;
	 }

	 IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width+20, height, Color.White);
	 _forecastMenu.DrawSupplyBar(Game1.spriteBatch, x+15, y+20, x+width, (Game1.tileSize / 2), model);
	}
 }
}