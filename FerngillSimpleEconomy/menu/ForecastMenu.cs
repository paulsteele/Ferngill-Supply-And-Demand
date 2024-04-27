using System;
using System.Collections.Generic;
using System.Linq;
using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace fse.core.menu;

public abstract class AbstractForecastMenu : IClickableMenu
{
	public abstract void TakeOverMenuTab(GameMenu gameMenu);
	public abstract void DrawSupplyBar(SpriteBatch batch, int startingX, int startingY, int endingX, int barHeight, ItemModel model);
}

public class ForecastMenu : AbstractForecastMenu
{
	private readonly IModHelper _helper;
	private readonly IEconomyService _economyService;
	private readonly IMonitor _monitor;
	private readonly IDrawTextHelper _drawTextHelper;
	private ItemModel[] _allItems;
	private Dictionary<int, string> _categories;
	private int _itemIndex;
	private int _maxNumberOfRows;
	private bool _isScrolling;
	private bool _isInCategoryDropdown;
	private bool _isInSortDropdown;
	private Texture2D _barBackgroundTexture;
	private Texture2D _barForegroundTexture;
	private ClickableTextureComponent _upArrow;
	private ClickableTextureComponent _downArrow;
	private ClickableTextureComponent _scrollbar;
	private Rectangle? _scrollbarRunner;
	private int _bottomIndex;
	private OptionsDropDown _categoryDropdown;
	private OptionsDropDown _sortDropdown;
	private OptionsCheckbox[] _seasonsCheckboxes;
	private ClickableTextureComponent _exitButton;
	private bool _drawn;

	private string None => _helper.Translation.Get("fse.forecast.menu.sort.none");
	private string Name => _helper.Translation.Get("fse.forecast.menu.sort.name");
	private string MarketPrice => _helper.Translation.Get("fse.forecast.menu.sort.marketPrice");
	private string MarketPricePerDay => _helper.Translation.Get("fse.forecast.menu.sort.marketPricePerDay");
	private string Supply => _helper.Translation.Get("fse.forecast.menu.sort.supply");
	private string DailyChange => _helper.Translation.Get("fse.forecast.menu.sort.delta");
		
	private readonly List<string> _sortOptions = new() { "None", nameof(Name), nameof(Supply), nameof(DailyChange), nameof(MarketPrice), nameof(MarketPricePerDay) };
	private readonly List<string> _sortDisplayOptions;
	private string _chosenSort;
	private int _chosenCategory;
	private Seasons _chosenSeasons = Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter;
	private GameMenu _hiddenMenu;

	private static int? _cachedChosenCategory;
	private static string _cachedChosenSort;

	private const int Divider1 = 340;
	private const int Divider2 = 560;
	private const int Divider3 = 780;
	private const int DividerWidth = 32;

	public ForecastMenu(
		IModHelper helper,
		IEconomyService economyService,
		IMonitor monitor,
		IDrawTextHelper drawTextHelper
	)
	{
		_helper = helper;
		_economyService = economyService;
		_monitor = monitor;
		_drawTextHelper = drawTextHelper;

		_sortDisplayOptions = new List<string> { None, Name, Supply, DailyChange, MarketPrice, MarketPricePerDay };

		_chosenSeasons = Utility.getSeasonNumber(Game1.currentSeason) switch {
			0 => Seasons.Spring,
			1 => Seasons.Summer,
			2 => Seasons.Fall,
			3 => Seasons.Winter,
			_ => _chosenSeasons,
		};

		_chosenSort = string.IsNullOrWhiteSpace(_cachedChosenSort) ? _sortOptions.First() : _cachedChosenSort ;
			
		if (economyService.Loaded)
		{
			_categories = economyService.GetCategories().GroupBy(pair => pair.Value).ToDictionary(pairs => pairs.First().Key, pairs => pairs.First().Value);
			_chosenCategory = _cachedChosenCategory ?? economyService.GetCategories().Keys.First();
			SetupItemsWithSort();
		}
		else
		{
			_categories = new Dictionary<int, string>();
			_allItems = Array.Empty<ItemModel>();
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		_barBackgroundTexture = null;
		_barForegroundTexture = null;
		_upArrow = null;
		_downArrow = null;
		_scrollbar = null;
		_scrollbarRunner = null;
		_categoryDropdown = null;
		_sortDropdown = null;
		_seasonsCheckboxes = null;
		_exitButton = null;
		_drawn = false;
	}

	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		var startingIndex = _itemIndex;
		_itemIndex = BoundsHelper.EnsureBounds(_itemIndex + (direction < 0 ? 1 : -1), 0, _bottomIndex);
		if (startingIndex != _itemIndex)
		{
			Game1.playSound("shwip");
		}
	}

	public override void TakeOverMenuTab(GameMenu gameMenu)
	{
		_hiddenMenu = gameMenu;
		Game1.activeClickableMenu = this;
			
		gameMenu.invisible = true;
		gameMenu.upperRightCloseButton.visible = false;
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		var startingIndex = _itemIndex;
		if (!_drawn)
		{
			return;
		}
			
		if (_categoryDropdown.bounds.Contains(x, y))
		{
			_categoryDropdown.receiveLeftClick(x, y);
			_isInCategoryDropdown = true;
		}
		else if (_sortDropdown.bounds.Contains(x, y))
		{
			_sortDropdown.receiveLeftClick(x, y);
			_isInSortDropdown = true;
		}
		else if (_upArrow.containsPoint(x, y))
		{
			_itemIndex = BoundsHelper.EnsureBounds(_itemIndex - 1, 0, _bottomIndex);
		}
		else if (_downArrow.containsPoint(x, y))
		{
			_itemIndex = BoundsHelper.EnsureBounds(_itemIndex + 1, 0, _bottomIndex);
		}
		else if (_scrollbar.containsPoint(x, y))
		{
			_isScrolling = true;
		}
		else if (_exitButton.containsPoint(x, y))
		{
			if (_hiddenMenu != null)
			{
				_hiddenMenu.invisible = false;
				_hiddenMenu.upperRightCloseButton.visible = true;

				Game1.activeClickableMenu = _hiddenMenu;
			}
		}

		var seasonsChanged = false;
		for (var i = 0; i < _seasonsCheckboxes.Length; i++)
		{
			var checkbox = _seasonsCheckboxes[i];

			if (!checkbox.bounds.Contains(x, y))
			{
				continue;
			}

			var flag = i switch
			{
				0 => Seasons.Spring,
				1 => Seasons.Summer,
				2 => Seasons.Fall,
				3 => Seasons.Winter,
				_ => Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter,
			};

			seasonsChanged = true;
					
			if (_chosenSeasons.HasFlag(flag))
			{
				_chosenSeasons -= flag;
			}
			else
			{
				_chosenSeasons |= flag;
			}
		}

		if (seasonsChanged)
		{
			SetupItemsWithSort();
		}

		if (startingIndex != _itemIndex)
		{
			Game1.playSound("shwip");
		}
	}

	public override void releaseLeftClick(int x, int y)
	{
		base.releaseLeftClick(x, y);
		_isScrolling = false;
		if (!_drawn)
		{
			return;
		}

		// ReSharper disable once InvertIf
		if (_isInCategoryDropdown)
		{
			_categoryDropdown.leftClickReleased(x, y);

			if (_categoryDropdown.dropDownOptions.Count > _categoryDropdown.selectedOption)
			{
				if (int.TryParse(_categoryDropdown.dropDownOptions[_categoryDropdown.selectedOption], out var result))
				{
					_chosenCategory = result;
					_cachedChosenCategory = _chosenCategory;
					SetupItemsWithSort();
					_itemIndex = 0;
				}
			}
			_isInCategoryDropdown = false;
		}
			
		// ReSharper disable once InvertIf
		if (_isInSortDropdown)
		{
			_sortDropdown.leftClickReleased(x, y);

			if (_sortDropdown.dropDownOptions.Count > _sortDropdown.selectedOption)
			{
				_chosenSort = _sortDropdown.dropDownOptions[_sortDropdown.selectedOption];
				_cachedChosenSort = _chosenSort;
				SetupItemsWithSort();
			}
			_isInSortDropdown = false;
		}
	}

	public override void leftClickHeld(int x, int y)
	{
		base.leftClickHeld(x, y);
		if (!_drawn)
		{
			return;
		}
		if (_isInCategoryDropdown)
		{
			_categoryDropdown.leftClickHeld(x, y);
			return;
		}
			
		if (_isInSortDropdown)
		{
			_sortDropdown.leftClickHeld(x, y);
			return;
		}
			
		if (!_isScrolling)
		{
			return;
		}

		if (!_scrollbarRunner.HasValue)
		{
			return;
		}

		var startingIndex = _itemIndex;

		if (y < _scrollbarRunner.Value.Y)
		{
			_itemIndex = 0;
		}

		if (y > _scrollbarRunner.Value.Y + _scrollbarRunner.Value.Height)
		{
			_itemIndex = _bottomIndex;
		}
			
		var totalBarLength = _scrollbarRunner.Value.Height - _scrollbar.bounds.Height;
		var step = totalBarLength / _bottomIndex;

		var relativeMousePos = y - _scrollbarRunner.Value.Y;

		_itemIndex = BoundsHelper.EnsureBounds(relativeMousePos / step, 0, _bottomIndex);

		if (_itemIndex != startingIndex)
		{
			Game1.playSound("shwip");
		}
	}

	public override void draw(SpriteBatch batch)
	{
		SetupPositionAndSize();
		DrawBackground(batch);
		DrawTitle(batch);
		DrawScrollBar(batch);
		DrawPartitions(batch);

		DrawHeader(batch);

		for (var i = 0; i < _maxNumberOfRows; i++)
		{
			if (_itemIndex + i < _allItems.Length)
			{
				DrawRow(batch, _allItems[_itemIndex + i], i, xPositionOnScreen, yPositionOnScreen, width);
			}
		}
		DrawCategoryDropdown(batch);
		DrawSortingDropdown(batch);
		DrawSeasonsCheckbox(batch);
		DrawExitButton(batch);
		DrawMouse(batch);
		_drawn = true;
	}

	private void SetupPositionAndSize()
	{
		const int xPadding = 100;
		const int yPadding = 50;
		width = Math.Min(Game1.uiViewport.Width - 2 * xPadding, 1920);
		height = Math.Min(Game1.uiViewport.Height - 2 * yPadding, 2000);

		_maxNumberOfRows = (height - 310) / 140;
		_bottomIndex = Math.Max(_allItems.Length - _maxNumberOfRows, 1);

		xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
		yPositionOnScreen = (Game1.uiViewport.Height - height) / 2;
	}
	private void DrawBackground(SpriteBatch batch)
	{
		Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
	}

	private void DrawPartitions(SpriteBatch batch)
	{
		const int offset = 115;

		height -= offset;
		yPositionOnScreen += offset;
		drawHorizontalPartition(batch, yPositionOnScreen + 60, true );
		drawHorizontalPartition(batch, yPositionOnScreen + 150, true );
		drawVerticalPartition(batch, xPositionOnScreen + Divider1, true);
		drawVerticalPartition(batch, xPositionOnScreen + Divider2, true);
		drawVerticalPartition(batch, xPositionOnScreen + Divider3, true);
		height += offset;
		yPositionOnScreen -= offset;
	}
			
	private void DrawHeader(SpriteBatch batch)
	{
		var yLoc = yPositionOnScreen + 254;
			
		_drawTextHelper.DrawAlignedText(batch, xPositionOnScreen + DividerWidth + (Divider1 / 2), yLoc, _helper.Translation.Get("fse.forecast.menu.header.item"), DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.Middle, false);
		_drawTextHelper.DrawAlignedText(batch, xPositionOnScreen + DividerWidth + Divider1 + ((Divider2 - Divider1) / 2), yLoc, _helper.Translation.Get("fse.forecast.menu.header.sellPrice"), DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.Middle, false);
		_drawTextHelper.DrawAlignedText(batch, xPositionOnScreen + DividerWidth + Divider2 + ((Divider3 - Divider2) / 2), yLoc, _helper.Translation.Get("fse.forecast.menu.header.sellPrice"), DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.Start, false);
		_drawTextHelper.DrawAlignedText(batch, xPositionOnScreen + DividerWidth + Divider2 + ((Divider3 - Divider2) / 2), yLoc, _helper.Translation.Get("fse.forecast.menu.header.perDay"), DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.End, false);
		_drawTextHelper.DrawAlignedText(batch, xPositionOnScreen + Divider3 + ((width - Divider3) / 2), yLoc, _helper.Translation.Get("fse.forecast.menu.header.supply"), DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.Start, false);
		_drawTextHelper.DrawAlignedText(batch, xPositionOnScreen + Divider3 + ((width - Divider3) / 2), yLoc, _helper.Translation.Get("fse.forecast.menu.header.supplyDescriptor"), DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.End, false);
	}

	private void DrawTitle(SpriteBatch batch)
	{
		_drawTextHelper.DrawAlignedText(batch, xPositionOnScreen + width / 2, yPositionOnScreen, _helper.Translation.Get("fse.forecast.menu.header.title"), DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.End, true);
	}

	private void DrawSeasonsCheckbox(SpriteBatch batch)
	{
		var yLoc = yPositionOnScreen + 153;
		var xCenterLoc = xPositionOnScreen + width / 2;
			
		if (_seasonsCheckboxes == null)
		{
			_seasonsCheckboxes = new[]
			{
				new OptionsCheckbox(string.Empty, (int)Seasons.Spring, xCenterLoc, yLoc),
				new OptionsCheckbox(string.Empty, (int)Seasons.Summer, xCenterLoc, yLoc),
				new OptionsCheckbox(string.Empty, (int)Seasons.Fall, xCenterLoc, yLoc),
				new OptionsCheckbox(string.Empty, (int)Seasons.Winter, xCenterLoc, yLoc),
			};

			var checkWidth = _seasonsCheckboxes[0].bounds.Width;

			_seasonsCheckboxes[0].bounds.X -= checkWidth * 2 + 9;
			_seasonsCheckboxes[1].bounds.X -= checkWidth * 1 + 1;
			_seasonsCheckboxes[2].bounds.X += 3;
			_seasonsCheckboxes[3].bounds.X += checkWidth * 1 + 9;
		}
			
		_seasonsCheckboxes[0].isChecked = _chosenSeasons.HasFlag(Seasons.Spring);
		_seasonsCheckboxes[1].isChecked = _chosenSeasons.HasFlag(Seasons.Summer);
		_seasonsCheckboxes[2].isChecked = _chosenSeasons.HasFlag(Seasons.Fall);
		_seasonsCheckboxes[3].isChecked = _chosenSeasons.HasFlag(Seasons.Winter);

		for (var i = 0; i < _seasonsCheckboxes.Length; i++)
		{
			var checkbox = _seasonsCheckboxes[i];
			checkbox.draw(batch, 0, 0);

			var xLoc = checkbox.bounds.X + (checkbox.bounds.Width / 2) - 18;

			batch.Draw(Game1.mouseCursors, new Vector2(xLoc, yPositionOnScreen + 115), new Rectangle(406, 441 + i * 8, 12, 8), Color.White, 0.0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
		}
	}

	private void DrawCategoryDropdown(SpriteBatch batch)
	{
		if (_categoryDropdown == null)
		{
			_categoryDropdown = new OptionsDropDown(
				string.Empty, 
				-999, 
				xPositionOnScreen + 50, 
				yPositionOnScreen + 153
			)
			{
				dropDownOptions = _categories.Keys.Select(i => i.ToString()).ToList(),
				dropDownDisplayOptions = _categories.Values.ToList(),
			};
				
			var index = _categoryDropdown.dropDownOptions.FindIndex(m => m.Equals(_chosenCategory.ToString()));
			if (index > -1)
			{
				_categoryDropdown.selectedOption = index;
			}

			_categoryDropdown.RecalculateBounds();
		}
			
		var xLoc = _categoryDropdown.bounds.X + (_categoryDropdown.bounds.Width / 2);
			
		_drawTextHelper.DrawAlignedText(batch, xLoc, yPositionOnScreen + 105, _helper.Translation.Get("fse.forecast.menu.header.category"), DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.End, false);
		_categoryDropdown.draw(batch, 0, 0);
	}
		
	private void DrawExitButton(SpriteBatch batch)
	{
		_exitButton ??= new ClickableTextureComponent(
			new Rectangle(
				xPositionOnScreen + width - 36,
				yPositionOnScreen - 8, 48, 48
			),
			Game1.mouseCursors,
			new Rectangle(337, 494, 12, 12), 4f);

		_exitButton.draw(batch);
	}
		
	private void DrawSortingDropdown(SpriteBatch batch)
	{
		if (_sortDropdown == null)
		{
			_sortDropdown = new OptionsDropDown(
				string.Empty, 
				-999, 
				xPositionOnScreen + width - 50, 
				yPositionOnScreen + 153
			)
			{
				dropDownOptions = _sortOptions,
				dropDownDisplayOptions = _sortDisplayOptions,
			};
				
			var index = _sortDropdown.dropDownOptions.FindIndex(m => m.Equals(_chosenSort));
			if (index > -1)
			{
				_sortDropdown.selectedOption = index;
			}

			_sortDropdown.bounds.X -= _sortDropdown.bounds.Width;
			_sortDropdown.dropDownBounds.X -= _sortDropdown.bounds.Width;
			_sortDropdown.RecalculateBounds();
				
		}
		var xLoc = _sortDropdown.bounds.X + (_sortDropdown.bounds.Width / 2);
			
		_drawTextHelper.DrawAlignedText(batch, xLoc, yPositionOnScreen + 105, _helper.Translation.Get("fse.forecast.menu.header.sortBy"), DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.End, false);
		_sortDropdown.draw(batch, 0, 0);
	}

	private void DrawRow(SpriteBatch batch, ItemModel model, int rowNumber, int startingX, int startingY, int rowWidth, int rowHeight = 100, int padding = 40)
	{
		var obj = model.GetObjectInstance();

		var x = startingX + padding;
		var y = startingY + 270 + padding + (rowHeight + padding) * rowNumber;

		var textCenterLine = y + rowHeight / 2;

		obj.drawInMenu(batch, new Vector2(x, y + 10), 1);

		if (rowNumber != _maxNumberOfRows - 1)
		{
			drawHorizontalPartition(batch, y + rowHeight - 5, true );
		}

		DrawSupplyBar(batch,x + Divider3 + 10, y + 10,  x + rowWidth - 10 - padding * 2, (Game1.tileSize / 2), model);

		var splitPoint = obj.DisplayName.LastIndexOf(" ", StringComparison.Ordinal);
		if (splitPoint == -1)
		{
			splitPoint = obj.DisplayName.LastIndexOf("_", StringComparison.Ordinal);
		}
		if (splitPoint == -1)
		{
			_drawTextHelper.DrawAlignedText(batch, x + Game1.tileSize, textCenterLine, obj.DisplayName, DrawTextHelper.DrawTextAlignment.Start, DrawTextHelper.DrawTextAlignment.Middle, false);
		}
		else
		{
			var firstLine = obj.DisplayName[..splitPoint];
			var secondLine = obj.DisplayName[(splitPoint + 1)..];
				
			_drawTextHelper.DrawAlignedText(batch, x + Game1.tileSize + 5, textCenterLine, firstLine, DrawTextHelper.DrawTextAlignment.Start, DrawTextHelper.DrawTextAlignment.Start, false);
			_drawTextHelper.DrawAlignedText(batch, x + Game1.tileSize + 5, textCenterLine, secondLine, DrawTextHelper.DrawTextAlignment.Start, DrawTextHelper.DrawTextAlignment.End, false);
		}

		var pricePerDay = _economyService.GetPricePerDay(model);

		var pricePerDayDisplay = pricePerDay != -1 ? $"{pricePerDay}" : "---";

		_drawTextHelper.DrawAlignedText(batch, xPositionOnScreen + DividerWidth + Divider1 + ((Divider2 - Divider1) / 2), textCenterLine, $"{obj.sellToStorePrice()}", DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.Middle, false);
		_drawTextHelper.DrawAlignedText(batch, xPositionOnScreen + DividerWidth + Divider2 + ((Divider3 - Divider2) / 2), textCenterLine, pricePerDayDisplay, DrawTextHelper.DrawTextAlignment.Middle, DrawTextHelper.DrawTextAlignment.Middle, false);
	}

	public override void DrawSupplyBar(SpriteBatch batch, int startingX, int startingY, int endingX, int barHeight, ItemModel model)
	{
		var barWidth = ((endingX - startingX) / 10) * 10;
		var percentage = Math.Min(model.Supply / (float)ConfigModel.Instance.MaxCalculatedSupply, 1);

		if (_barBackgroundTexture == null || _barForegroundTexture == null)
		{
			_barBackgroundTexture = new Texture2D(Game1.graphics.GraphicsDevice, barWidth, barHeight);
			_barForegroundTexture = new Texture2D(Game1.graphics.GraphicsDevice, barWidth, barHeight);
			var borderColor = Color.DarkGoldenrod;
			var foregroundColor = new Color(150, 150, 150);
			var backgroundTick = new Color(50, 50, 50);
			var foregroundTick = new Color(120, 120, 120);
			
			var backgroundColors = new Color[barWidth * barHeight];
			var foregroundColors = new Color[barWidth * barHeight];

			for (var x = 0; x < barWidth; x++)
			{
				for (var y = 0; y < barHeight; y++)
				{
					var background = Color.Transparent;
					var foreground = Color.Transparent;
					
					if (x < 2 || y < 2 || x > barWidth - 3 || y > barHeight - 3)
					{
						background = borderColor;
					}
					else if (x % (barWidth/ 10) == 0)
					{
						background = backgroundTick;
						foreground = foregroundTick;
					}
					else
					{
						foreground = foregroundColor;
					}
					
					backgroundColors[x + y * barWidth] = background;
					foregroundColors[x + y * barWidth] = foreground;
				}
			}
			
			_barBackgroundTexture.SetData(backgroundColors);
			_barForegroundTexture.SetData(foregroundColors);
		}

		var barColor = new Color((byte)(255 * percentage), (byte)(255 * (1 - percentage)), 0);

		var fullRect = new Rectangle(startingX, startingY + Game1.tileSize / 2, barWidth,  barHeight);
		var percentageRect = new Rectangle(startingX, startingY + Game1.tileSize / 2, (int) (barWidth * percentage), barHeight);
		batch.Draw(_barBackgroundTexture, fullRect, new Rectangle(0, 0, barWidth, barHeight), Color.White);
		batch.Draw(_barForegroundTexture, percentageRect, new Rectangle(0, 0, percentageRect.Width, barHeight), barColor);
			
		DrawDeltaArrows(batch, model, percentageRect, barHeight);
	}

	private static void DrawDeltaArrows(SpriteBatch batch, ItemModel model, Rectangle percentageRect, int barHeight)
	{
		var location = new Rectangle(percentageRect.X + percentageRect.Width - (int)(Game1.tileSize * .3) + 15,
			percentageRect.Y - barHeight, 5 * Game1.pixelZoom, 5 * Game1.pixelZoom);

		if (model.DailyDelta < 0)
		{
			var leftArrow = new ClickableTextureComponent("up-arrow", location, "", "", Game1.mouseCursors,
				new Rectangle(352, 495, 12, 11), Game1.pixelZoom * .75f);
			leftArrow.bounds.X -= 30;
			if (model.DailyDelta < -20)
			{
				leftArrow.bounds.X += 10;
				leftArrow.draw(batch);
			}

			if (model.DailyDelta < -10)
			{
				leftArrow.bounds.X += 10;
				leftArrow.draw(batch);
			}

			leftArrow.bounds.X += 10;
			leftArrow.draw(batch);
		}
		else
		{
			var rightArrow = new ClickableTextureComponent("down-arrow", location, "", "", Game1.mouseCursors,
				new Rectangle(365, 495, 12, 11), Game1.pixelZoom * .75f);
			if (model.DailyDelta > 20)
			{
				rightArrow.bounds.X -= 10;
				rightArrow.draw(batch);
			}

			if (model.DailyDelta > 10)
			{
				rightArrow.bounds.X -= 10;
				rightArrow.draw(batch);
			}

			rightArrow.bounds.X -= 10;
			rightArrow.draw(batch);
		}
	}

	private void DrawScrollBar(SpriteBatch batch)
	{
		var padding = 15;
		if (_upArrow == null || _downArrow == null || _scrollbar == null || _scrollbarRunner == null)
		{
				
			_upArrow = new ClickableTextureComponent("up-arrow", new Rectangle(xPositionOnScreen + width + padding, yPositionOnScreen + Game1.tileSize + padding, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
			_downArrow = new ClickableTextureComponent("down-arrow", new Rectangle(xPositionOnScreen + width + padding, yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);
			_scrollbar = new ClickableTextureComponent("scrollbar", new Rectangle(_upArrow.bounds.X + Game1.pixelZoom * 3, _upArrow.bounds.Y + _upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(435, 463, 6, 10), Game1.pixelZoom);
			_scrollbarRunner = new Rectangle(_scrollbar.bounds.X, _upArrow.bounds.Y + _upArrow.bounds.Height + Game1.pixelZoom, _scrollbar.bounds.Width, height - Game1.tileSize * 2 - _upArrow.bounds.Height - Game1.pixelZoom * 2 - padding - 3);
		}
			
		var totalBarLength = _scrollbarRunner.Value.Height - _scrollbar.bounds.Height;
		var step = totalBarLength / _bottomIndex;

		if (_itemIndex == _bottomIndex)
		{
			_scrollbar.bounds.Y = _scrollbarRunner.Value.Y + _scrollbarRunner.Value.Height - _scrollbar.bounds.Height;
		}
		else
		{
			_scrollbar.bounds.Y = _upArrow.bounds.Y + _upArrow.bounds.Height + Game1.pixelZoom + (step * _itemIndex);
		}
			
		drawTextureBox(
			batch, 
			Game1.mouseCursors, 
			new Rectangle(403, 383, 6, 6), 
			_scrollbarRunner.Value.X, 
			_scrollbarRunner.Value.Y, 
			_scrollbarRunner.Value.Width, 
			_scrollbarRunner.Value.Height, 
			Color.White, 
			Game1.pixelZoom, 
			false
		);
		_upArrow.draw(batch);
		_downArrow.draw(batch);
		_scrollbar.draw(batch);
	}

	private void DrawMouse(SpriteBatch batch)
	{
		if (!Game1.options.hardwareCursor)
		{
			batch.Draw(
				Game1.mouseCursors, 
				new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), 
				Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16), 
				Color.White, 
				0f, 
				Vector2.Zero, 
				Game1.pixelZoom + Game1.dialogueButtonScale / 150f, 
				SpriteEffects.None, 
				1f
			);
		}
	}


	private void SetupItemsWithSort()
	{
		var items = _economyService.GetItemsForCategory(_chosenCategory).ToList();

		switch (_chosenSort)
		{
			case nameof(Name):
			{
				items.Sort((a, b) =>
					string.Compare(a.GetObjectInstance().DisplayName, b.GetObjectInstance().DisplayName, StringComparison.Ordinal)
				);
				break;
			}
			case nameof(Supply):
			{
				items.Sort((a, b) => a.Supply - b.Supply);
				break;
			}
			case nameof(DailyChange):
			{
				items.Sort((a, b) => a.DailyDelta - b.DailyDelta);
				break;
			}
			case nameof(MarketPrice):
			{
				items.Sort((a, b) => b.GetPrice(b.GetObjectInstance().Price) - a.GetPrice(a.GetObjectInstance().Price));
				break;
			}
			case nameof(MarketPricePerDay):
			{
				items.Sort((a, b) => _economyService.GetPricePerDay(b) - _economyService.GetPricePerDay(a));
				break;
			}
		}

		items = items.Where(i => _economyService.ItemValidForSeason(i, _chosenSeasons)).ToList();

		_allItems = items.ToArray();
	}
}