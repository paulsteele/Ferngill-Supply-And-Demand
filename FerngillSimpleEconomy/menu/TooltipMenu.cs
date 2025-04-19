using System.Linq;
using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace fse.core.menu;

public interface ITooltipMenu
{
	void PreRenderHud(RenderingHudEventArgs e);
	void PostRenderHud(RenderedHudEventArgs e);
	void PostRenderGui(RenderedActiveMenuEventArgs e);
}

public class TooltipMenu(
	IModHelper helper,
	IEconomyService econService,
	IDrawSupplyBarHelper drawSupplyBarHelper,
	IBetterGameMenuService betterGameMenuService
	) : ITooltipMenu
{
	private Item? _toolbarItem;
	private readonly bool _isUiInfoSuiteLoaded = helper.ModRegistry.IsLoaded("Annosz.UiInfoSuite2");

	public void PreRenderHud(RenderingHudEventArgs e)
	{
		if (!ConfigModel.Instance.EnableTooltip)
		{
			return;
		}

		_toolbarItem = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault()?.hoverItem;
	}

	public void PostRenderHud(RenderedHudEventArgs e)
	{
		if (!ConfigModel.Instance.EnableTooltip || Game1.activeClickableMenu != null || _toolbarItem == null)
		{
			return;
		}

		PopulateHoverTextBoxAndDraw(_toolbarItem);
		_toolbarItem = null;
	}

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

	private Item? GetHoveredItemFromMenu(IClickableMenu menu)
	{
		var page = betterGameMenuService.GetCurrentPage(menu);
		if (page is InventoryPage inventoryPage)
		{
			return inventoryPage.hoveredItem;
		}

		if (menu is MenuWithInventory inventoryMenu)
		{
			return inventoryMenu.hoveredItem;
		}

		return null;
	}

	private void PopulateHoverTextBoxAndDraw(Item item)
	{
		if (item is not Object obj)
		{
			return;
		}
		var model = econService.GetItemModelFromObject(obj);

		if (model == null)
		{
			return;
		}

		DrawHoverTextBox(model);
	}

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
		drawSupplyBarHelper.DrawSupplyBar(Game1.spriteBatch, x+15, y+20, x+width, (Game1.tileSize / 2), model);
	}
}