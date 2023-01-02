using fsd.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace fsd.core.menu
{
	public class ForecastMenu : IClickableMenu
	{
		private readonly EconomyService _economyService;
		private readonly IMonitor _monitor;

		public ForecastMenu(
			EconomyService economyService,
			IMonitor monitor)
		{
			_economyService = economyService;
			_monitor = monitor;
		}

		public override void draw(SpriteBatch batch)
		{
			SetupPositionAndSize();
			
			DrawBackground(batch);
			DrawTitle(batch);
			DrawMouse(batch);
		}

		private void SetupPositionAndSize()
		{
			var xPadding = 300;
			var yPadding = 300;
			width = Game1.uiViewport.Width - 2 * xPadding;
			height = Game1.uiViewport.Height - 2 * yPadding;

			xPositionOnScreen = xPadding;
			yPositionOnScreen = yPadding;
		}
		private void DrawBackground(SpriteBatch batch)
		{
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
		}

		private void DrawTitle(SpriteBatch batch)
		{
			DrawBoxWithAlignedText(xPositionOnScreen + width / 2, yPositionOnScreen, "Ferngill Economy Forecast", Alignment.Middle, Alignment.End, batch);
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