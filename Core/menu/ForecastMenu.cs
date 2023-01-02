using System;
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
		private ItemModel _testItem;
		private Texture2D _barBackgroundTexture;
		private Texture2D _barForegroundTexture;

		public ForecastMenu(
			EconomyService economyService,
			IMonitor monitor)
		{
			_economyService = economyService;
			_monitor = monitor;

			_testItem = new ItemModel{ObjectId = 24, Supply = 300, DailyDelta = 22};
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			_barBackgroundTexture = null;
			_barForegroundTexture = null;
		}

		public override void draw(SpriteBatch batch)
		{
			SetupPositionAndSize();
			DrawBackground(batch);
			DrawTitle(batch);
			DrawScrollBar(batch);
			DrawRow(batch, _testItem, 0);
			DrawRow(batch, _testItem, 1);
			DrawRow(batch, _testItem, 2);
			DrawMouse(batch);
		}

		private void SetupPositionAndSize()
		{
			const int xPadding = 100;
			const int yPadding = 50;
			width = Math.Min(Game1.uiViewport.Width - 2 * xPadding, 1920);
			height = Math.Min(Game1.uiViewport.Height - 2 * yPadding, 1080);

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
			DrawSupplyBar(batch, x + Game1.tileSize, y, xPositionOnScreen + width - padding * 2, Math.Min(model.Supply / (float) ItemModel.MaxCalculatedSupply, 1));
		}

		private void DrawSupplyBar(SpriteBatch batch, int startingX, int startingY, int endingX, float percentage)
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
		}

		private void DrawScrollBar(SpriteBatch batch)
		{
			var padding = 15;
			var upArrow = new ClickableTextureComponent("up-arrow", new Rectangle(xPositionOnScreen + width + padding, yPositionOnScreen + Game1.tileSize + padding, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
			var downArrow = new ClickableTextureComponent("down-arrow", new Rectangle(xPositionOnScreen + width + padding, yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);
			var scrollbar = new ClickableTextureComponent("scrollbar", new Rectangle(upArrow.bounds.X + Game1.pixelZoom * 3, upArrow.bounds.Y + upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(435, 463, 6, 10), Game1.pixelZoom);
			var scrollbarRunner = new Rectangle(scrollbar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + Game1.pixelZoom, scrollbar.bounds.Width, height - Game1.tileSize * 2 - upArrow.bounds.Height - Game1.pixelZoom * 2 - padding - 3);
			
			drawTextureBox(
				batch, 
				Game1.mouseCursors, 
				new Rectangle(403, 383, 6, 6), 
				scrollbarRunner.X, 
				scrollbarRunner.Y, 
				scrollbarRunner.Width, 
				scrollbarRunner.Height, 
				Color.White, 
				Game1.pixelZoom, 
				false
			);
			upArrow.draw(batch);
			downArrow.draw(batch);
			scrollbar.draw(batch);
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