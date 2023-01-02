using System;
using System.Collections.Generic;
using fsd.core.helpers;
using fsd.core.models;
using fsd.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace fsd.core.menu
{
	public class ForecastMenu : IClickableMenu
	{
		private readonly EconomyService _economyService;
		private readonly IMonitor _monitor;
		private ItemModel[] _allItems;
		private int _itemIndex;
		private int _maxNumberOfRows = 0;
		private bool _isScrolling;
		private Texture2D _barBackgroundTexture;
		private Texture2D _barForegroundTexture;
		private ClickableTextureComponent _upArrow;
		private ClickableTextureComponent _downArrow;
		private ClickableTextureComponent _scrollbar;
		private Rectangle? _scrollbarRunner;
		private int _bottomIndex;

		public ForecastMenu(
			EconomyService economyService,
			IMonitor monitor)
		{
			_economyService = economyService;
			_monitor = monitor;

			var items = new List<ItemModel>();

			for (var i = 0; i < 20; i++)
			{
				items.Add(new ItemModel { ObjectId = 24 + i, Supply = 200, DailyDelta = 30 });
			}

			_allItems = items.ToArray();
			_itemIndex = 4;
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
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			_itemIndex = BoundsHelper.EnsureBounds(_itemIndex + (direction < 0 ? 1 : -1), 0, _bottomIndex);
			Game1.playSound("shwip");
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);

			if (_upArrow.containsPoint(x, y))
			{
				_itemIndex = BoundsHelper.EnsureBounds(_itemIndex - 1, 0, _bottomIndex);
				Game1.playSound("shwip");
			}
			
			if (_downArrow.containsPoint(x, y))
			{
				_itemIndex = BoundsHelper.EnsureBounds(_itemIndex + 1, 0, _bottomIndex);
				Game1.playSound("shwip");
			}

			if (_scrollbar.containsPoint(x, y))
			{
				_isScrolling = true;
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			_isScrolling = false;
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
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

			for (var i = 0; i < _maxNumberOfRows; i++)
			{
				if (_itemIndex + i < _allItems.Length)
				{
					DrawRow(batch, _allItems[_itemIndex + i], i);
				}
			}
			DrawMouse(batch);
		}

		private void SetupPositionAndSize()
		{
			const int xPadding = 100;
			const int yPadding = 50;
			width = Math.Min(Game1.uiViewport.Width - 2 * xPadding, 1920);
			height = Math.Min(Game1.uiViewport.Height - 2 * yPadding, 2000);

			_maxNumberOfRows = (height - 140) / 140;
			_bottomIndex = _allItems.Length - _maxNumberOfRows;

			xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
			yPositionOnScreen = (Game1.uiViewport.Height - height) / 2;
		}
		private void DrawBackground(SpriteBatch batch)
		{
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
		}

		private void DrawTitle(SpriteBatch batch)
		{
			DrawBoxWithAlignedText(xPositionOnScreen + width / 2, yPositionOnScreen, "Ferngill Economic Forecast", Alignment.Middle, Alignment.End, batch);
		}

		private void DrawRow(SpriteBatch batch, ItemModel model, int rowNumber)
		{
			var obj = new Object(model.ObjectId, 1);

			var padding = 40;
			var rowHeight = 100;
			var x = xPositionOnScreen + padding;
			var y = yPositionOnScreen + 80 + padding + (rowHeight + padding) * rowNumber;

			obj.drawInMenu(batch, new Vector2(x, y), 1);
			
			DrawSupplyBar(batch, x + (int) (Game1.tileSize * 1.2), y, xPositionOnScreen + width - padding * 2, Math.Min(model.Supply / (float)ItemModel.MaxCalculatedSupply, 1), model.DailyDelta);
			Utility.drawTextWithShadow(batch, obj.Name, Game1.dialogueFont, new Vector2(x, y + Game1.tileSize), Game1.textColor);
		}

		private void DrawSupplyBar(SpriteBatch batch, int startingX, int startingY, int endingX, float percentage, int delta)
		{
			var barWidth = ((endingX - startingX) / 10) * 10;
			var barHeight = Game1.tileSize / 2;

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
			
			//delta arrows
			var location = new Rectangle(percentageRect.X + percentageRect.Width - (int)(Game1.tileSize * .3), percentageRect.Y - barHeight, 5 * Game1.pixelZoom, 5 * Game1.pixelZoom);

			if (delta < 0)
			{
				var leftArrow = new ClickableTextureComponent("up-arrow", location, "", "", Game1.mouseCursors, new Rectangle(352, 495, 12, 11), Game1.pixelZoom * .75f);
				leftArrow.draw(batch);
				if (delta < -20)
				{
					leftArrow.bounds.X -= 10;
					leftArrow.draw(batch);
				}
				if (delta < -40)
				{
					leftArrow.bounds.X -= 10;
					leftArrow.draw(batch);
				}
			}
			else
			{
				var rightArrow = new ClickableTextureComponent("down-arrow", location, "", "", Game1.mouseCursors, new Rectangle(365, 495, 12, 11), Game1.pixelZoom * .75f);
				rightArrow.draw(batch);
				if (delta > 20)
				{
					rightArrow.bounds.X += 10;
					rightArrow.draw(batch);
				}
				if (delta > 40)
				{
					rightArrow.bounds.X += 10;
					rightArrow.draw(batch);
				}
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
			//this.SetScrollBarToCurrentIndex();
			
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
		
		private void DrawBoxWithAlignedText(
			int x, 
			int y, 
			string text, 
			Alignment horizontalAlignment,
			Alignment verticalAlignment,
			SpriteBatch batch
		)
		{
			var textBounds = Game1.dialogueFont.MeasureString(text);

			var newX = horizontalAlignment switch
			{
				Alignment.Middle => (int) (x - textBounds.X / 2),
				_ => x,
			};

			var newY = verticalAlignment switch
			{
				Alignment.End => y,
				_ => y,
			};

			var padding = 16;
			
			//drawTextureBox(batch, Game1.menuTexture, new Rectangle(0, 0, (int)textBounds.X, (int)textBounds.Y), (int)newX, (int)newY, (int)textBounds.X, (int)textBounds.Y, Game1.textColor);
			Game1.drawDialogueBox(
				newX - Game1.tileSize / 2 - padding / 2, 
				(newY - (int) textBounds.Y / 1) - Game1.tileSize / 2 - padding, 
				(int)textBounds.X + Game1.tileSize + padding, 
				(int)((2 * Game1.tileSize) + textBounds.Y
				), false, true);
			Utility.drawTextWithShadow(batch, text, Game1.dialogueFont, new Vector2(newX, newY), Game1.textColor);
		}
		
		private enum Alignment
		{
			Start,
			Middle,
			End
		}
	}

}