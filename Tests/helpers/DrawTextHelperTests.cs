using fse.core.helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using StardewValley;
using Tests.HarmonyMocks;

namespace Tests.helpers;

public class DrawTextHelperTests : HarmonyTestBase
{
	private DrawTextHelper _helper;
	
	[SetUp]
	public override void Setup()
	{
		base.Setup();

		Game1.dialogueFont = new SpriteFont(null, null, null, null, 0, 0, null, null);
		_helper = new DrawTextHelper();
	}

	[TestCase(
		0, 
		0, 
		"test", 
		100, 
		100,
		DrawTextHelper.DrawTextAlignment.Start,
		DrawTextHelper.DrawTextAlignment.Start,
		true,
		-40,
		-248,
		180,
		228,
		0,
		-100
	)]
	[TestCase(
		0, 
		0, 
		"test", 
		100, 
		100,
		DrawTextHelper.DrawTextAlignment.Start,
		DrawTextHelper.DrawTextAlignment.Start,
		false,
		0,
		0,
		0,
		0,
		0,
		-100
	)]
	[TestCase(
		100, 
		200, 
		"test", 
		300, 
		500,
		DrawTextHelper.DrawTextAlignment.Start,
		DrawTextHelper.DrawTextAlignment.Start,
		true,
		60,
		-848,
		380,
		628,
		100,
		-300
	)]
	public void ShouldDrawText(
		int x, 
		int y, 
		string text, 
		int measuredWidth,
		int measuredHeight,
		DrawTextHelper.DrawTextAlignment horizontalDrawTextAlignment,
		DrawTextHelper.DrawTextAlignment verticalDrawTextAlignment,
		bool addBox,
		float expectedBoxX,
		float expectedBoxY,
		float expectedBoxWidth,
		float expectedBoxHeight,
		float expectedTextX,
		float expectedTextY
	)
	{
		HarmonySpriteFont.MeasureStringResult = new Vector2(measuredWidth, measuredHeight);
		
		_helper.DrawAlignedText(
			new SpriteBatch(null),
			x,
			y,
			text,
			horizontalDrawTextAlignment,
			verticalDrawTextAlignment,
			addBox
		);

		if (addBox)
		{
			Assert.That(HarmonyGame.DrawDialogueBoxCalls, Has.Count.EqualTo(1));
			var call = HarmonyGame.DrawDialogueBoxCalls[0];
			Assert.Multiple(() =>
			{
				Assert.That(call.x, Is.EqualTo(expectedBoxX), ".x");
				Assert.That(call.y, Is.EqualTo(expectedBoxY), ".y");
				Assert.That(call.width, Is.EqualTo(expectedBoxWidth), ".width");
				Assert.That(call.height, Is.EqualTo(expectedBoxHeight), ".height");
				Assert.That(call.speaker, Is.False, ".speaker");
				Assert.That(call.drawOnlyBox, Is.True, ".drawOnlyBox");
			});
		}
		else
		{
			Assert.That(HarmonyGame.DrawDialogueBoxCalls, Has.Count.Zero);
		}

		Assert.That(HarmonyUtility.DrawTextWithShadowCalls, Has.Count.EqualTo(1));
		var textCall = HarmonyUtility.DrawTextWithShadowCalls[0];
		Assert.Multiple(() =>
		{
			Assert.That(textCall.text, Is.EqualTo(text));
			Assert.That(textCall.position, Is.EqualTo(new Vector2(expectedTextX, expectedTextY)));
		});
	}
}