namespace Tests.helpers;

public class DrawSupplyBarHelperTests
{
	
	
		// // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		// var supplyBarCalls = HarmonySpriteBatch.DrawCalls[_batch].Where(b => b.texture == Game1.staminaRect).ToArray();
		//
		// //doesn't seem that useful to unit test graphics being drawn precisely. Reconsider if bugs arise.
		// Assert.That(supplyBarCalls, Has.Length.GreaterThanOrEqualTo(18 * expectedRows));
		//
		// var negativeDeltaArrows = HarmonyClickableTextureComponent.DrawCalls.Keys.FirstOrDefault(c => c.name == "left-arrow");
		//
		// if (expectedLeftArrowCalls != 0)
		// {
		// 	Assert.Multiple(() =>
		// 	{
		// 		Assert.That(HarmonyClickableTextureComponent.DrawCalls[negativeDeltaArrows!], Is.EqualTo(expectedLeftArrowCalls));
		// 		Assert.That(negativeDeltaArrows!.bounds.X, Is.EqualTo(expectedLeftArrowLocation));
		// 		Assert.That(negativeDeltaArrows.bounds.Y, Is.EqualTo(411));
		// 	});
		// }
		// else
		// {
		// 	Assert.That(negativeDeltaArrows, Is.Null);
		// }
		//
		// var positiveDeltaArrows =
		// 	HarmonyClickableTextureComponent.DrawCalls.Keys.FirstOrDefault(c => c.name == "right-arrow");
		//
		// if (expectedRightArrowCalls != 0)
		// {
		// 	Assert.Multiple(() =>
		// 	{
		// 		Assert.That(HarmonyClickableTextureComponent.DrawCalls[positiveDeltaArrows!], Is.EqualTo(expectedRightArrowCalls));
		// 		Assert.That(positiveDeltaArrows!.bounds.X, Is.EqualTo(expectedRightArrowLocation));
		// 		Assert.That(positiveDeltaArrows.bounds.Y, Is.EqualTo(411));
		// 	});
		// }
		// else
		// {
		// 	Assert.That(positiveDeltaArrows, Is.Null);
		// }
		//
		// if (expectedRows > 1)
		// {
		// 	var negativeDeltaArrow2 = HarmonyClickableTextureComponent.DrawCalls.Keys.Where(c => c.name == "left-arrow").Skip(1)
		// 		.FirstOrDefault();
		//
		// 	if (expectedLeftArrowCalls != 0)
		// 	{
		// 		Assert.Multiple(() =>
		// 		{
		// 			Assert.That(HarmonyClickableTextureComponent.DrawCalls[negativeDeltaArrow2!], Is.EqualTo(expectedLeftArrowCalls));
		// 			Assert.That(negativeDeltaArrow2!.bounds.X, Is.EqualTo(expectedLeftArrowLocation));
		// 			Assert.That(negativeDeltaArrow2.bounds.Y, Is.EqualTo(510));
		// 		});
		// 	}
		// 	else
		// 	{
		// 		Assert.That(negativeDeltaArrow2, Is.Null);
		// 	}
		//
		// 	var positiveDeltaArrow2 = HarmonyClickableTextureComponent.DrawCalls.Keys.Where(c => c.name == "right-arrow").Skip(1)
		// 		.FirstOrDefault();
		//
		// 	if (expectedRightArrowCalls != 0)
		// 	{
		// 		Assert.Multiple(() =>
		// 		{
		// 			Assert.That(HarmonyClickableTextureComponent.DrawCalls[positiveDeltaArrow2!], Is.EqualTo(expectedRightArrowCalls));
		// 			Assert.That(positiveDeltaArrow2!.bounds.X, Is.EqualTo(expectedRightArrowLocation));
		// 			Assert.That(positiveDeltaArrow2.bounds.Y, Is.EqualTo(531));
		// 		});
		// 	}
		// 	else
		// 	{
		// 		Assert.That(positiveDeltaArrows, Is.Null);
		// 	}
		// }
		//
}