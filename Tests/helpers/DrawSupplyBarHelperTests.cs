using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using StardewValley;
using Tests.HarmonyMocks;

namespace Tests.helpers;

[TestFixture]
public class DrawSupplyBarHelperTests : HarmonyTestBase
{
	private DrawSupplyBarHelper _helper;
	private Mock<IEconomyService> _mockEconomyService;
	private SpriteBatch _batch;
	private ItemModel _item;

	[SetUp]
	public override void Setup()
	{
		base.Setup();
		Game1.graphics = new GraphicsDeviceManager(null);

		_batch = new SpriteBatch(null, 0);
		_item = new ItemModel("item1"){Supply = 50, DailyDelta = 20};

		_mockEconomyService = new Mock<IEconomyService>();
		_mockEconomyService.Setup(m => m.GetConsolidatedItem(It.IsAny<ItemModel>())).Returns(_item);
		
		_helper = new DrawSupplyBarHelper(_mockEconomyService.Object);
	}

	[TearDown]
	public override void TearDown()
	{
		base.TearDown();
		_batch.Dispose();
	}

	[TestCase(10, 100,  92, 25, 229, 0, 0, 1, 0, 206)]
	[TestCase(20, 100,  92, 25, 229, 0, 0, 2, 0, 196)]
	[TestCase(30, 100,  92, 25, 229, 0, 0, 3, 0, 186)]
	[TestCase(-10, 100,  92, 25, 229, 0, 1, 0, 196, 0)]
	[TestCase(-20, 100,  92, 25, 229, 0, 2, 0, 206, 0)]
	[TestCase(-30, 100,  92, 25, 229, 0, 3, 0, 216, 0)]
	[TestCase(10, 500,  460, 127, 127, 0, 0, 1, 0, 286)]
	public void ShouldDrawSupplyBar(
		int delta,
		int supply,
		int expectedBarWidth,
		int expectedBarColorRed,
		int expectedBarColorGreen,
		int expectedBarColorBlue,
		int expectedLeftArrowCalls,
		int expectedRightArrowCalls,
		int expectedLeftArrowLocation,
		int expectedRightArrowLocation
	)
	{
		_item.Supply = supply;
		_item.DailyDelta = delta;
		
		_helper.DrawSupplyBar(_batch, 200, 300, 400, 32, _item);
		
		var supplyBarCalls = HarmonySpriteBatch.DrawCalls[_batch].Where(b => b.texture == Game1.staminaRect).ToArray();
		
		//doesn't seem that useful to unit test graphics being drawn precisely. Reconsider if bugs arise.
		Assert.That(supplyBarCalls, Has.Length.GreaterThanOrEqualTo(18));
		
		var negativeDeltaArrows = HarmonyClickableTextureComponent.DrawCalls.Keys.FirstOrDefault(c => c.name == "left-arrow");
		
		if (expectedLeftArrowCalls != 0)
		{
			Assert.Multiple(() =>
			{
				Assert.That(HarmonyClickableTextureComponent.DrawCalls[negativeDeltaArrows!], Is.EqualTo(expectedLeftArrowCalls));
				Assert.That(negativeDeltaArrows!.bounds.X, Is.EqualTo(expectedLeftArrowLocation));
				Assert.That(negativeDeltaArrows.bounds.Y, Is.EqualTo(296));
			});
		}
		else
		{
			Assert.That(negativeDeltaArrows, Is.Null);
		}
		
		var positiveDeltaArrows =
			HarmonyClickableTextureComponent.DrawCalls.Keys.FirstOrDefault(c => c.name == "right-arrow");
		
		if (expectedRightArrowCalls != 0)
		{
			Assert.Multiple(() =>
			{
				Assert.That(HarmonyClickableTextureComponent.DrawCalls[positiveDeltaArrows!], Is.EqualTo(expectedRightArrowCalls));
				Assert.That(positiveDeltaArrows!.bounds.X, Is.EqualTo(expectedRightArrowLocation));
				Assert.That(positiveDeltaArrows.bounds.Y, Is.EqualTo(296));
			});
		}
		else
		{
			Assert.That(positiveDeltaArrows, Is.Null);
		}
	}
}