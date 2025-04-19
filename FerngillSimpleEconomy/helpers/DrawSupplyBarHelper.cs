using System;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace fse.core.helpers;

public interface IDrawSupplyBarHelper
{
	public void DrawSupplyBar(SpriteBatch batch, int startingX, int startingY, int endingX, int barHeight, ItemModel model);
}

public class DrawSupplyBarHelper(EconomyService economyService) : IDrawSupplyBarHelper
{ 
	private float? _breakEvenSupply;
	
	public void DrawSupplyBar(SpriteBatch batch, int startingX, int startingY, int endingX, int barHeight, ItemModel originalModel)
	{
		var model = economyService.GetConsolidatedItem(originalModel);
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

		_breakEvenSupply ??= economyService.GetBreakEvenSupply();
		
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
}