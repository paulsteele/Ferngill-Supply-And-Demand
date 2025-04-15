using System;
using System.Collections.Generic;
using System.Linq;
using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace fse.core.menu;

public abstract class AbstractForecastMenu : IClickableMenu
{
	public abstract void TakeOverMenuTab(GameMenu gameMenu);

	public abstract void SetBetterGameMenu(IBetterGameMenuService service);

	public abstract void DrawSupplyBar(SpriteBatch batch, int startingX, int startingY, int endingX, int barHeight, ItemModel model);
}

public class ForecastMenu : AbstractForecastMenu
{
	private readonly IModHelper _helper;
	private readonly IEconomyService _economyService;
	private readonly IMonitor _monitor;
	private readonly IDrawTextHelper _drawTextHelper;
	private IBetterGameMenuService _betterGameMenuService;
	private ItemModel[] _allItems;
	private readonly Dictionary<int, string> _categories;
	private int _itemIndex;
	private int _maxNumberOfRows;
	private bool _isScrolling;
	private bool _isInCategoryDropdown;
	private bool _isInSortDropdown;
	private ClickableTextureComponent _upArrow;
	private ClickableTextureComponent _downArrow;
	private ClickableTextureComponent _scrollbar;
	private Rectangle? _scrollbarRunner;
	private int _bottomIndex;
	private OptionsDropDown _categoryDropdown;
	private OptionsDropDown _sortDropdown;
	private OptionsTextEntry _optionsTextEntry;
	private OptionsCheckbox[] _seasonsCheckboxes;
	private ClickableTextureComponent _exitButton;
	private bool _drawn;

	private string None => _helper.Translation.Get("fse.forecast.menu.sort.none");
	private string Name => _helper.Translation.Get("fse.forecast.menu.sort.name");
	private string MarketPrice => _helper.Translation.Get("fse.forecast.menu.sort.marketPrice");
	private string MarketPricePerDay => _helper.Translation.Get("fse.forecast.menu.sort.marketPricePerDay");
	private string Supply => _helper.Translation.Get("fse.forecast.menu.sort.supply");
	private string DailyChange => _helper.Translation.Get("fse.forecast.menu.sort.delta");
		
	private readonly List<string> _sortOptions = ["None", nameof(Name), nameof(Supply), nameof(DailyChange), nameof(MarketPrice), nameof(MarketPricePerDay)];
	private readonly List<string> _sortDisplayOptions;
	private string _chosenSort;
	private int _chosenCategory;
	private Seasons _chosenSeasons;
	private GameMenu _hiddenMenu;

	private static int? _cachedChosenCategory;
	private static string _cachedChosenSort;
	private static string _textFilter;
	private static int _controllerIndex;
	
	private float? _breakEvenSupply;

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

		_sortDisplayOptions = [None, Name, Supply, DailyChange, MarketPrice, MarketPricePerDay];

		_chosenSeasons = SeasonHelper.GetCurrentSeason();

		_chosenSort = string.IsNullOrWhiteSpace(_cachedChosenSort) ? _sortOptions.First() : _cachedChosenSort ;
			
		if (economyService.Loaded)
		{
			_categories = new Dictionary<int, string>
			{
				{ int.MinValue, helper.Translation.Get("fse.forecast.menu.allCategory") },
			};
				
			economyService.GetCategories();
			_categories = _categories.Concat(economyService.GetCategories()).ToDictionary(g => g.Key, g => g.Value);
				
			_categories = _categories.GroupBy(pair => pair.Value).ToDictionary(pairs => pairs.First().Key, pairs => pairs.First().Value);
			_chosenCategory = _cachedChosenCategory ?? int.MinValue;
			SetupItemsWithSort();
		}
		else
		{
			_categories = new Dictionary<int, string>();
			_allItems = [];
		}
	}

	public override void SetBetterGameMenu(IBetterGameMenuService service)
	{
		_betterGameMenuService = service;
	}


	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		_upArrow = null;
		_optionsTextEntry = null;
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

	public override bool readyToClose() => !_optionsTextEntry?.textBox.Selected ?? true;

	public override void receiveGamePadButton(Buttons button)
	{
		// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
		switch (button)
		{
			case Buttons.LeftTrigger:
				GoBackToPreviousMenu();
				break;
			case Buttons.LeftThumbstickRight:
			case Buttons.DPadRight:
				if (_isInCategoryDropdown || _isInSortDropdown)
				{
					break;
				}
				NavigateToNextControllerElement(true);
				break;
			case Buttons.LeftThumbstickLeft:
			case Buttons.DPadLeft:
				if (_isInCategoryDropdown || _isInSortDropdown)
				{
					break;
				}
				NavigateToNextControllerElement(false);
				break;
			case Buttons.LeftThumbstickDown:
			case Buttons.DPadDown:
				if (_isInCategoryDropdown)
				{
					_categoryDropdown.selectedOption = (_categoryDropdown.selectedOption + 1 + _categoryDropdown.dropDownOptions.Count) % _categoryDropdown.dropDownOptions.Count;
				}

				if (_isInSortDropdown)
				{
					_sortDropdown.selectedOption = (_sortDropdown.selectedOption + 1 + _sortDropdown.dropDownOptions.Count) % _sortDropdown.dropDownOptions.Count;
				}

				break;
			case Buttons.LeftThumbstickUp:
			case Buttons.DPadUp:
				if (_isInCategoryDropdown)
				{
					_categoryDropdown.selectedOption = (_categoryDropdown.selectedOption - 1 + _categoryDropdown.dropDownOptions.Count) % _categoryDropdown.dropDownOptions.Count;
				}

				if (_isInSortDropdown)
				{
					_sortDropdown.selectedOption = (_sortDropdown.selectedOption - 1 + _sortDropdown.dropDownOptions.Count) % _sortDropdown.dropDownOptions.Count;
				}

				break;
		}

		base.receiveGamePadButton(button);
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
			GoBackToPreviousMenu();
		}
		_optionsTextEntry?.receiveLeftClick(x, y);

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

	private void NavigateToNextControllerElement(bool forward)
	{
		(Rectangle baseRect, int xOffset, int yOffset)[] elements = [ 
			(_categoryDropdown.bounds, _categoryDropdown.bounds.Width - 32, _categoryDropdown.bounds.Height / 2), 
			(_seasonsCheckboxes[0].bounds, _seasonsCheckboxes[0].bounds.Width / 2, _seasonsCheckboxes[0].bounds.Height / 2),
			(_seasonsCheckboxes[1].bounds, _seasonsCheckboxes[1].bounds.Width / 2, _seasonsCheckboxes[1].bounds.Height / 2),
			(_seasonsCheckboxes[2].bounds, _seasonsCheckboxes[2].bounds.Width / 2, _seasonsCheckboxes[2].bounds.Height / 2),
			(_seasonsCheckboxes[3].bounds,_seasonsCheckboxes[2].bounds.Width / 2, _seasonsCheckboxes[2].bounds.Height / 2),
			(_sortDropdown.bounds, _sortDropdown.bounds.Width - 32, _sortDropdown.bounds.Height / 2),
		];
		
		var offset = forward ? 1 : -1;

		_controllerIndex = (_controllerIndex + offset + elements.Length) % elements.Length;
		
		var element = elements[_controllerIndex];

		Mouse.SetPosition(element.baseRect.X + element.xOffset, element.baseRect.Y + element.yOffset);
	}

	private void GoBackToPreviousMenu()
	{
			if (_hiddenMenu != null)
			{
				_hiddenMenu.invisible = false;
				_hiddenMenu.upperRightCloseButton.visible = true;

				Game1.activeClickableMenu = _hiddenMenu;
			}
			else if (_betterGameMenuService != null)
			{
				_betterGameMenuService.SwitchToLastTab();
			}
			else
			{
				exitThisMenu();
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
		var step = totalBarLength / (float)_bottomIndex;

		var relativeMousePos = y - _scrollbarRunner.Value.Y;


		if (step == 0)
		{
			// should not be possible but a user report has a stacktrace that says it is.

			return;
		}

		_itemIndex = BoundsHelper.EnsureBounds((int)Math.Round(relativeMousePos / step) , 0, _bottomIndex);

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
		DrawSearchBar(batch);

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

		_maxNumberOfRows = (height - 355) / 120;
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
		const int offset = 160;

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
		var yLoc = yPositionOnScreen + 299;
			
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
				dropDownDisplayOptions = _categories.Values.Select(s => string.IsNullOrWhiteSpace(s) ? _helper.Translation.Get("fse.forecast.menu.basic.category") : s).ToList(),
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

	private void DrawSearchBar(SpriteBatch batch)
	{
		_optionsTextEntry ??= new OptionsTextEntry(string.Empty, 0, 0, 0)
		{
			textBox =
			{
				Text = _textFilter,
			},
		};
		var x = xPositionOnScreen + (width / 2) - (_optionsTextEntry.textBox.Width / 2);
		
		_optionsTextEntry.bounds.X = x;
		_optionsTextEntry.bounds.Y = 250;
		
		_optionsTextEntry.draw(batch, 0, 0);

		if (_optionsTextEntry.textBox.Text == _textFilter)
		{
			return;
		}

		_textFilter = _optionsTextEntry.textBox.Text;
		SetupItemsWithSort();
		_itemIndex = 0;
	}

	private void DrawRow(SpriteBatch batch, ItemModel model, int rowNumber, int startingX, int startingY, int rowWidth, int rowHeight = 80, int padding = 40)
	{
		var obj = model.GetObjectInstance();

		var x = startingX + padding;
		var y = startingY + 315 + padding + (rowHeight + padding) * rowNumber;

		var textCenterLine = y + rowHeight / 2;

		obj.drawInMenu(batch, new Vector2(x, y + 10), 1);

		if (rowNumber != _maxNumberOfRows - 1)
		{
			drawHorizontalPartition(batch, y + rowHeight - 5, true );
		}

		DrawSupplyBar(batch,x + Divider3 + 5, y + 10,  x + rowWidth - 15 - padding * 2, (Game1.tileSize / 2), model);
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

	public override void DrawSupplyBar(SpriteBatch batch, int startingX, int startingY, int endingX, int barHeight, ItemModel originalModel)
	{
		var model = _economyService.GetConsolidatedItem(originalModel);
		var barWidth = ((endingX - startingX) / 10) * 10;
		var percentage = Math.Min(model.Supply / (float)ConfigModel.Instance.MaxCalculatedSupply, 1);

		var percentageRect = new Rectangle(startingX, startingY + Game1.tileSize / 2, (int) (barWidth * percentage), barHeight);
		var percentageWidth = (int)(barWidth * percentage);
			
		var color1 = new Color((int)(60 + percentage * 120), (int)(180 - percentage * 120), (int)(80 - percentage * 80));
		var color2 = new Color((int)(percentage * 113), (int)(113 - percentage * 113), (int)(62 - percentage * 62));
		var color3 = new Color((int)(percentage * 80), (int)(80 - percentage * 80), (int)(50 - percentage * 50));
		var color4 = new Color((int)(percentage * 60), (int)(60 - percentage * 60), (int)(30 - percentage * 30));
		
		var y = startingY + 32;
		
		// bar background
		batch.Draw(Game1.staminaRect, new Rectangle(startingX  - 1, y + 4, barWidth + 4, 40), Color.Black * 0.35f);
		batch.Draw(Game1.staminaRect, new Rectangle(startingX + 4, y , barWidth + 4, 40), new Color(60, 60, 25));
		batch.Draw(Game1.staminaRect, new Rectangle(startingX + 8, y + 4, barWidth + 4 - 12, 32), new Color(173, 129, 79));

		y += 4;
		
		// bar foreground
		if (percentageWidth > 4)
		{
			batch.Draw(Game1.staminaRect, new Rectangle(startingX + 8, y , percentageWidth, 32), color2);
			batch.Draw(Game1.staminaRect, new Rectangle(startingX + 8, y + 4, 4, 28), color3);
			batch.Draw(Game1.staminaRect, new Rectangle(startingX + 8, y + 28, percentageWidth - 8, 4), color3);
			batch.Draw(Game1.staminaRect, new Rectangle(startingX + 12, y , percentageWidth - 4, 4), color1);
			batch.Draw(Game1.staminaRect, new Rectangle(startingX + percentageWidth, y, 4, 28), color1);
			batch.Draw(Game1.staminaRect, new Rectangle(startingX + 4 + percentageWidth, y, 4, 32), color4);
		}

		// ticks 
		for (var i = 1; i < 10; i++)
		{
			var tickX = startingX + ((barWidth / 10) * i);
			if (percentageRect.X + percentageRect.Width > tickX)
			{
				batch.Draw(Game1.staminaRect, new Rectangle(tickX , y, 4, 28), color1);
			}
			batch.Draw(Game1.staminaRect, new Rectangle(tickX + 4, y, 4, 32), color4);
		}

		_breakEvenSupply ??= _economyService.GetBreakEvenSupply();
		
		if (_breakEvenSupply.Value > 0)
		{
			var evenX = (int) Math.Floor((startingX + barWidth * _breakEvenSupply.Value / ConfigModel.Instance.MaxCalculatedSupply));
			
			batch.Draw(Game1.mouseCursors, new Rectangle(evenX, startingY + 12, 18, 16), new Rectangle(232, 347, 9, 8), Color.White);
		}
		DrawDeltaArrows(batch, model, percentageRect, barHeight);
	}

	private static void DrawDeltaArrows(SpriteBatch batch, ItemModel model, Rectangle percentageRect, int barHeight)
	{
		var location = new Rectangle(percentageRect.X + percentageRect.Width - (int)(Game1.tileSize * .3) + 15,
			percentageRect.Y - barHeight - 4, 5 * Game1.pixelZoom, 5 * Game1.pixelZoom);
		
		if (model.DailyDelta < 0)
		{
			var leftArrow = new ClickableTextureComponent("left-arrow", location, "", "", Game1.mouseCursors,
				new Rectangle(352, 495, 12, 11), Game1.pixelZoom * .75f);
			leftArrow.bounds.X -= 30;
			if (model.DailyDelta < -2 * ConfigModel.Instance.DeltaArrow)
			{
				leftArrow.bounds.X += 10;
				leftArrow.draw(batch);
			}

			if (model.DailyDelta < -1 * ConfigModel.Instance.DeltaArrow)
			{
				leftArrow.bounds.X += 10;
				leftArrow.draw(batch);
			}

			leftArrow.bounds.X += 10;
			leftArrow.draw(batch);
		}
		else
		{
			var rightArrow = new ClickableTextureComponent("right-arrow", location, "", "", Game1.mouseCursors,
				new Rectangle(365, 495, 12, 11), Game1.pixelZoom * .75f);
			if (model.DailyDelta > 2 * ConfigModel.Instance.DeltaArrow)
			{
				rightArrow.bounds.X -= 10;
				rightArrow.draw(batch);
			}

			if (model.DailyDelta > ConfigModel.Instance.DeltaArrow)
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
		var step = totalBarLength / (float)_bottomIndex;

		if (_itemIndex == _bottomIndex)
		{
			_scrollbar.bounds.Y = _scrollbarRunner.Value.Y + _scrollbarRunner.Value.Height - _scrollbar.bounds.Height;
		}
		else
		{
			_scrollbar.bounds.Y = _upArrow.bounds.Y + _upArrow.bounds.Height + Game1.pixelZoom + (int)Math.Round(step * _itemIndex);
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
		List<ItemModel> items;

		if (_chosenCategory == int.MinValue)
		{
			items = _economyService.GetCategories()
				.SelectMany(c => _economyService.GetItemsForCategory(c.Key))
				.ToList();
		}
		else
		{
			items = _economyService.GetItemsForCategory(_chosenCategory).ToList();
		}

		if (!string.IsNullOrWhiteSpace(_textFilter))
		{
			var filter = _textFilter.ToLower().Trim();
			items = items.Where(i => i.GetObjectInstance().DisplayName.ToLower().Trim().Contains(filter)).ToList();
		}

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
				items.Sort((a, b) => _economyService.GetConsolidatedItem(a).Supply - _economyService.GetConsolidatedItem(b).Supply);
				break;
			}
			case nameof(DailyChange):
			{
				items.Sort((a, b) => _economyService.GetConsolidatedItem(a).DailyDelta - _economyService.GetConsolidatedItem(b).DailyDelta);
				break;
			}
			case nameof(MarketPrice):
			{
				items.Sort((a, b) =>
				{
					var aObj = a.GetObjectInstance();
					var bObj = b.GetObjectInstance();
					return _economyService.GetPrice(bObj, bObj.sellToStorePrice()) - _economyService.GetPrice(aObj, aObj.sellToStorePrice());
				});
				break;
			}
			case nameof(MarketPricePerDay):
			{
				items.Sort((a, b) => _economyService.GetPricePerDay(b) - _economyService.GetPricePerDay(a));
				break;
			}
		}

		items = items.Where(i => _economyService.ItemValidForSeason(i, _chosenSeasons)).DistinctBy(i => i.ObjectId).ToList();

		_allItems = items.ToArray();
		_breakEvenSupply = _economyService.GetBreakEvenSupply();
	}
}