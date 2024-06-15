using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System;
using System.Collections.Generic;
using fse.core.menu;
using fse.core.models;
using fse.core.services;

namespace fse.core.tooltip
{
 internal class Tooltip
 {
	private static IModHelper _helper;
	private static AbstractForecastMenu _forecastMenu;
	private static IEconomyService _econService;
	private static IForecastMenuService _forecastMenuService;
	private static IMonitor _monitor;
	private Item toolbarItem;
	private bool isUiInfoSuiteLoaded;

	//Constructor
	public Tooltip(IModHelper helper, IEconomyService econService, IForecastMenuService forecastMenuService, IMonitor monitor)
	{
	 _helper = helper;
	 _econService = econService;
	 _forecastMenuService = forecastMenuService;
	 _monitor = monitor;

	 _forecastMenu = _forecastMenuService.CreateMenu();

	 isUiInfoSuiteLoaded = _helper.ModRegistry.IsLoaded("Annosz.UiInfoSuite2"); 

	 _helper.Events.Display.RenderingHud += PreRenderHud;
	 _helper.Events.Display.RenderedHud += PostRenderHud;
	 _helper.Events.Display.RenderedActiveMenu += PostRenderGui;
	}

	//Get hovered toolbar item
	public void PreRenderHud(object sender, RenderingHudEventArgs e)
	{
	 toolbarItem = null;
	 foreach (IClickableMenu menu in Game1.onScreenMenus)
	 {
		if (menu is Toolbar toolbar)
		{
		 toolbarItem = _helper.Reflection.GetField<Item>(menu, "hoverItem").GetValue();
		 return;
		}
	 }
	}

	//Render for toolbar item
	public void PostRenderHud(object sender, RenderedHudEventArgs e)
	{
	 if (Game1.activeClickableMenu == null && toolbarItem != null)
	 {
		PopulateHoverTextBoxAndDraw(toolbarItem);
		toolbarItem = null;
	 }
	}

	//Render for menu item
	public void PostRenderGui(object sender, RenderedActiveMenuEventArgs e)
	{
	 if (Game1.activeClickableMenu != null)
	 {
		Item item = this.GetHoveredItemFromMenu(Game1.activeClickableMenu);
		if (item != null)
		 PopulateHoverTextBoxAndDraw(item);
	 }
	}

	//Get hovered menu item
	private Item GetHoveredItemFromMenu(IClickableMenu menu)
	{
	 // game menu
	 if (menu is GameMenu gameMenu)
	 {
		IClickableMenu page = _helper.Reflection.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue()[gameMenu.currentTab];
		if (page is InventoryPage)
		 return _helper.Reflection.GetField<Item>(page, "hoveredItem").GetValue();
	 }
	 // from inventory UI (so things like shops and so on)
	 else if (menu is MenuWithInventory inventoryMenu)
	 {
		return inventoryMenu.hoveredItem;
	 }

	 return null;
	}

	//Populate and draw
	private void PopulateHoverTextBoxAndDraw(Item item)
	{
	 String itemid = item.QualifiedItemId;

	 //determine parent item id from tag (for wine,juice,etc.)
	 HashSet<string> itemtags = item.GetContextTags();
	 foreach (string tag in itemtags)
	 {
		 if (tag.StartsWith("preserve_sheet_index_")){
			itemid = tag.Split("_")[3];
		 }
	 }

	 ItemModel model = _econService.GetItemModelById(itemid);
	 if (model != null)
	 {
		Seasons allSeasons = Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter;
		if (_econService.ItemValidForSeason(model, allSeasons))
		{
		 this.DrawHoverTextBox(model);
		}
	 }

	}

	//Draw
	private void DrawHoverTextBox(ItemModel model)
	{
	 int width = 240;
	 int height = 110;

	 int x = (int)(Mouse.GetState().X / Game1.options.uiScale) - Game1.tileSize / 2 - width;
	 int y = (int)(Mouse.GetState().Y / Game1.options.uiScale) + Game1.tileSize / 3;

	 //So that the tooltips don't overlap
	 if ((isUiInfoSuiteLoaded))
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