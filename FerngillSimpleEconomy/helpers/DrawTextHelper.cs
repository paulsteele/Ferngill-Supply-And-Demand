using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace fse.core.helpers;

public interface IDrawTextHelper
{
	void DrawAlignedText(
		SpriteBatch batch,
		int x, 
		int y, 
		string text, 
		DrawTextHelper.DrawTextAlignment horizontalDrawTextAlignment,
		DrawTextHelper.DrawTextAlignment verticalDrawTextAlignment,
		bool addBox
	);
}

public class DrawTextHelper : IDrawTextHelper
{
	public void DrawAlignedText(
		SpriteBatch batch,
		int x, 
		int y, 
		string text, 
		DrawTextAlignment horizontalDrawTextAlignment,
		DrawTextAlignment verticalDrawTextAlignment,
		bool addBox
	)
	{
		var textBounds = Game1.dialogueFont.MeasureString(text);

		var newX = horizontalDrawTextAlignment switch
		{
			DrawTextAlignment.Middle => (int) (x - textBounds.X / 2),
			_ => x,
		};

		var newY = verticalDrawTextAlignment switch
		{
			DrawTextAlignment.End => y,
			DrawTextAlignment.Middle => (int) (y - textBounds.Y / 2),
			DrawTextAlignment.Start => (int) (y - textBounds.Y),
			_ => y,
		};

		var padding = 16;

		if (addBox)
		{
			Game1.drawDialogueBox(
				newX - Game1.tileSize / 2 - padding / 2, 
				(newY - (int) textBounds.Y / 1) - Game1.tileSize / 2 - padding, 
				(int)textBounds.X + Game1.tileSize + padding, 
				(int)((2 * Game1.tileSize) + textBounds.Y
				), false, true);
		}
		
		Utility.drawTextWithShadow(batch, text, Game1.dialogueFont, new Vector2(newX, newY), Game1.textColor);
	}
	public enum DrawTextAlignment
	{
		Start,
		Middle,
		End
	}
}