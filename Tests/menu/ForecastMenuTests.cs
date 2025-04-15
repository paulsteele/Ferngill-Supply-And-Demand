using fse.core.helpers;
using fse.core.menu;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using StardewModdingAPI;
using StardewValley;
using Tests.HarmonyMocks;
using Tests.Mocks;

namespace Tests.menu;

public class ForecastMenuTests : HarmonyTestBase
{
	private SpriteBatch _batch;
	private Mock<IDrawTextHelper> _drawTextHelperMock;
	private Mock<IEconomyService> _economyServiceMock;
	private Mock<IModHelper> _helperMock;
	private ForecastMenu _menu;
	private Mock<IMonitor> _monitorMock;

	[SetUp]
	public override void Setup()
	{
		base.Setup();

		_helperMock = new Mock<IModHelper>();
		_economyServiceMock = new Mock<IEconomyService>();
		_monitorMock = new Mock<IMonitor>();
		_drawTextHelperMock = new Mock<IDrawTextHelper>();

		_helperMock.Setup(m => m.Translation).Returns(new MockTranslationHelper());

		ConfigModel.Instance = new ConfigModel
		{
			MinDelta = 0,
			MaxDelta = 1000,
		};

		_economyServiceMock.Setup(x => x.Loaded).Returns(true);
		_economyServiceMock.Setup(x => x.GetCategories()).Returns(new Dictionary<int, string>
		{
			{ 1, "Category1" },
			{ 2, "Category2" },
			{ 3, "Category3" },
		});
		_economyServiceMock.Setup(m => m.GetItemsForCategory(1)).Returns(
			[
				new ItemModel { ObjectId = "1", Supply = 100, DailyDelta = 100 },
				new ItemModel { ObjectId = "2", Supply = 200, DailyDelta = 200 },
			]
		);
		_economyServiceMock.Setup(m => m.GetItemsForCategory(2)).Returns(
			[
				new ItemModel { ObjectId = "3", Supply = 300, DailyDelta = 300 },
				new ItemModel { ObjectId = "4", Supply = 400, DailyDelta = 400 },
				new ItemModel { ObjectId = "5", Supply = 500, DailyDelta = 500 },
			]
		);
		_economyServiceMock.Setup(m => m.GetItemsForCategory(3)).Returns(
			[
				new ItemModel { ObjectId = "6", Supply = 600, DailyDelta = 600 },
			]
		);

		_economyServiceMock.Setup(m => m.GetConsolidatedItem(It.IsAny<ItemModel>())).Returns<ItemModel>(i => i);

		_economyServiceMock.Setup(m => m.ItemValidForSeason(It.IsAny<ItemModel>(), It.IsAny<Seasons>())).Returns(true);

		HarmonyGame.GetOptionsResult = new Options();
		Game1.graphics = new GraphicsDeviceManager(null);

		_batch = new SpriteBatch(null, 0);
		_menu = new ForecastMenu(_helperMock.Object, _economyServiceMock.Object, _monitorMock.Object,
			_drawTextHelperMock.Object);
	}

	[TearDown]
	public void Teardown()
	{
		_batch.Dispose();
	}

	[TestCase(1080, 620, 880, 520, 100, 50)]
	[TestCase(10800, 6200, 1920, 2000, 4440, 2100)]
	public void ShouldSetupPositionAndSizeAndDrawBackground
	(
		int screenWidth,
		int screenHeight,
		int expectedWidth,
		int expectedHeight,
		int expectedX,
		int expectedY
	)
	{
		Game1.uiViewport.Width = screenWidth;
		Game1.uiViewport.Height = screenHeight;

		_menu.draw(_batch);

		var calls = HarmonyGame.DrawDialogueBoxCalls;

		Assert.Multiple(() =>
		{
			Assert.That(_menu.width, Is.EqualTo(expectedWidth), "Menu width does not match expectation");
			Assert.That(_menu.height, Is.EqualTo(expectedHeight), "Menu height does not match expectation");
			Assert.That(_menu.xPositionOnScreen, Is.EqualTo(expectedX), "Menu x-coordinate does not match expectation");
			Assert.That(_menu.yPositionOnScreen, Is.EqualTo(expectedY), "Menu y-coordinate does not match expectation");
			Assert.That(calls, Has.Count.EqualTo(1), "Dialogue box not drawn");
			Assert.That(calls[0].x, Is.EqualTo(expectedX), "Dialogue box x-coordinate does not match expectation");
			Assert.That(calls[0].y, Is.EqualTo(expectedY), "Dialogue box y-coordinate does not match expectation");
			Assert.That(calls[0].width, Is.EqualTo(expectedWidth), "Dialogue box width does not match expectation");
			Assert.That(calls[0].height, Is.EqualTo(expectedHeight), "Dialogue box height does not match expectation");
		});
	}

	[TestCase(1080, 620, 540, 50)]
	[TestCase(10800, 6200, 5400, 2100)]
	public void ShouldDrawTitleInCorrectPosition
	(
		int screenWidth,
		int screenHeight,
		int expectedX,
		int expectedY
	)
	{
		Game1.uiViewport.Width = screenWidth;
		Game1.uiViewport.Height = screenHeight;

		_menu.draw(_batch);

		_drawTextHelperMock.Verify(m => m.DrawAlignedText(
			_batch,
			expectedX,
			expectedY,
			"translation-fse.forecast.menu.header.title",
			DrawTextHelper.DrawTextAlignment.Middle,
			DrawTextHelper.DrawTextAlignment.End,
			true
		));
	}

	[TestCase(1080, 620, 2, 0, 995, 129, 44, 48, 995, 506, 44, 48, 1007, 181, 24, 40, 1007, 181, 24, 318)]
	[TestCase(10800, 6200, 2, 0, 6375, 2179, 44, 48, 6375, 4036, 44, 48, 6387, 2231, 24, 40, 6387, 2231, 24, 1798)]
	[TestCase(1080, 620, 10, 0, 995, 129, 44, 48, 995, 506, 44, 48, 1007, 181, 24, 40, 1007, 181, 24, 318)]
	[TestCase(10800, 6200, 10, 0, 6375, 2179, 44, 48, 6375, 4036, 44, 48, 6387, 2231, 24, 40, 6387, 2231, 24, 1798)]
	[TestCase(1080, 620, 10, 3, 995, 129, 44, 48, 995, 506, 44, 48, 1007, 274, 24, 40, 1007, 181, 24, 318)]
	[TestCase(10800, 6200, 10, 3, 6375, 2179, 44, 48, 6375, 4036, 44, 48, 6387, 3989, 24, 40, 6387, 2231, 24, 1798)]
	public void ShouldDrawScrollBarElementsInCorrectPosition
	(
		int screenWidth,
		int screenHeight,
		int numItems,
		int itemIndex,
		int upArrowExpectedX,
		int upArrowExpectedY,
		int upArrowExpectedWidth,
		int upArrowExpectedHeight,
		int downArrowExpectedX,
		int downArrowExpectedY,
		int downArrowExpectedWidth,
		int downArrowExpectedHeight,
		int barExpectedX,
		int barExpectedY,
		int barExpectedWidth,
		int barExpectedHeight,
		int runnerExpectedX,
		int runnerExpectedY,
		int runnerExpectedWidth,
		int runnerExpectedHeight
	)
	{
		Game1.uiViewport.Width = screenWidth;
		Game1.uiViewport.Height = screenHeight;

		var models = new List<ItemModel>();
		for (var i = 0; i < numItems; i++)
		{
			models.Add(new ItemModel { DailyDelta = i * 100, ObjectId = i.ToString(), Supply = i * 100 });
		}

		_economyServiceMock.Setup(m => m.GetItemsForCategory(1)).Returns(models.ToArray);
		_menu = new ForecastMenu(_helperMock.Object, _economyServiceMock.Object, _monitorMock.Object, _drawTextHelperMock.Object);

		_menu.draw(_batch);

		for (var i = 0; i < itemIndex; i++)
		{
			_menu.receiveScrollWheelAction(-1);
		}

		_menu.draw(_batch);

		var upArrow = HarmonyClickableTextureComponent.DrawCalls.First(p => p.Key.name == "up-arrow");
		var downArrow = HarmonyClickableTextureComponent.DrawCalls.First(p => p.Key.name == "down-arrow");
		var scrollbar = HarmonyClickableTextureComponent.DrawCalls.First(p => p.Key.name == "scrollbar");
		var runner = HarmonyIClickableMenu.DrawTextureBoxCalls.First().Value.First();

		Assert.Multiple(() =>
		{
			Assert.That(upArrow.Value, Is.EqualTo(2));
			Assert.That(downArrow.Value, Is.EqualTo(2));
			Assert.That(scrollbar.Value, Is.EqualTo(2));

			Assert.That(upArrow.Key.bounds.X, Is.EqualTo(upArrowExpectedX), "Up Arrow X position does not match expectation");
			Assert.That(upArrow.Key.bounds.Y, Is.EqualTo(upArrowExpectedY), "Up Arrow Y position does not match expectation");
			Assert.That(upArrow.Key.bounds.Width, Is.EqualTo(upArrowExpectedWidth), "Up Arrow width does not match expectation");
			Assert.That(upArrow.Key.bounds.Height, Is.EqualTo(upArrowExpectedHeight),
				"Up Arrow height does not match expectation");

			Assert.That(downArrow.Key.bounds.X, Is.EqualTo(downArrowExpectedX),
				"Down Arrow X position does not match expectation");
			Assert.That(downArrow.Key.bounds.Y, Is.EqualTo(downArrowExpectedY),
				"Down Arrow Y position does not match expectation");
			Assert.That(downArrow.Key.bounds.Width, Is.EqualTo(downArrowExpectedWidth),
				"Down Arrow width does not match expectation");
			Assert.That(downArrow.Key.bounds.Height, Is.EqualTo(downArrowExpectedHeight),
				"Down Arrow height does not match expectation");

			Assert.That(scrollbar.Key.bounds.X, Is.EqualTo(barExpectedX), "Scrollbar X position does not match expectation");
			Assert.That(scrollbar.Key.bounds.Y, Is.EqualTo(barExpectedY), "Scrollbar Y position does not match expectation");
			Assert.That(scrollbar.Key.bounds.Width, Is.EqualTo(barExpectedWidth), "Scrollbar width does not match expectation");
			Assert.That(scrollbar.Key.bounds.Height, Is.EqualTo(barExpectedHeight),
				"Scrollbar height does not match expectation");

			Assert.That(runner.x, Is.EqualTo(runnerExpectedX), "Runner X position does not match expectation");
			Assert.That(runner.y, Is.EqualTo(runnerExpectedY), "Runner Y position does not match expectation");
			Assert.That(runner.width, Is.EqualTo(runnerExpectedWidth), "Runner width does not match expectation");
			Assert.That(runner.height, Is.EqualTo(runnerExpectedHeight), "Runner height does not match expectation");
		});
	}

	[TestCase(1080, 620, 270, 360, 440, 660, 880, 360, 210)]
	[TestCase(10800, 6200, 2320, 2410, 4780, 5000, 5220, 1840, 2260)]
	public void ShouldDrawStaticPartitions
	(
		int screenWidth,
		int screenHeight,
		int expectedFirstHorizontalY,
		int expectedSecondHorizontalY,
		int expectedFirstVerticalX,
		int expectedSecondVerticalX,
		int expectedThirdVerticalX,
		int expectedModifiedHeight,
		int expectedModifiedY
	)
	{
		Game1.uiViewport.Width = screenWidth;
		Game1.uiViewport.Height = screenHeight;

		_menu.draw(_batch);

		var horizontalCalls = HarmonyIClickableMenu.DrawHoriztonalPartitionCalls[_batch];
		var verticalCalls = HarmonyIClickableMenu.DrawVerticalPartitionCalls[_batch];

		var firstHorizontal = horizontalCalls[0];
		var secondHorizontal = horizontalCalls[1];

		var firstVertical = verticalCalls[0];
		var secondVertical = verticalCalls[1];
		var thirdVertical = verticalCalls[2];

		Assert.Multiple(() =>
		{
			Assert.That(firstHorizontal.yPosition, Is.EqualTo(expectedFirstHorizontalY), "First horizontal partition Y position does not match expectation");
			Assert.That(secondHorizontal.yPosition, Is.EqualTo(expectedSecondHorizontalY), "Second horizontal partition Y position does not match expectation");

			Assert.That(firstVertical.xPosition, Is.EqualTo(expectedFirstVerticalX), "First vertical partition X position does not match expectation");
			Assert.That(secondVertical.xPosition, Is.EqualTo(expectedSecondVerticalX), "Second vertical partition X position does not match expectation");
			Assert.That(thirdVertical.xPosition, Is.EqualTo(expectedThirdVerticalX), "Third vertical partition X position does not match expectation");

			Assert.That(firstHorizontal.height, Is.EqualTo(expectedModifiedHeight), "First horizontal partition height does not match expectation");
			Assert.That(firstHorizontal.yPositionOnScreen, Is.EqualTo(expectedModifiedY), "First horizontal partition y position on screen does not match expectation");
			Assert.That(secondHorizontal.height, Is.EqualTo(expectedModifiedHeight), "Second horizontal partition height does not match expectation");
			Assert.That(secondHorizontal.yPositionOnScreen, Is.EqualTo(expectedModifiedY), "Second horizontal partition y position on screen does not match expectation");
			Assert.That(firstVertical.height, Is.EqualTo(expectedModifiedHeight), "First vertical partition height does not match expectation");
			Assert.That(firstVertical.yPositionOnScreen, Is.EqualTo(expectedModifiedY), "First vertical partition y position on screen does not match expectation");
			Assert.That(secondVertical.height, Is.EqualTo(expectedModifiedHeight), "Second vertical partition height does not match expectation");
			Assert.That(secondVertical.yPositionOnScreen, Is.EqualTo(expectedModifiedY), "Second vertical partition y position on screen does not match expectation");
			Assert.That(thirdVertical.height, Is.EqualTo(expectedModifiedHeight), "Third vertical partition height does not match expectation");
			Assert.That(thirdVertical.yPositionOnScreen, Is.EqualTo(expectedModifiedY), "Third vertical partition y position on screen does not match expectation");
		});
	}

	[TestCase(1080, 620, 349, 302, 582, 802, 930)]
	[TestCase(10800, 6200, 2399, 4642, 4922, 5142, 5790)]
	public void ShouldDrawHeader
	(
		int screenWidth,
		int screenHeight,
		int expectedY,
		int expectedHeaderX,
		int expectedSellPriceX,
		int expectedPerDayX,
		int expectedSupplyX
	)
	{
		Game1.uiViewport.Width = screenWidth;
		Game1.uiViewport.Height = screenHeight;

		_menu.draw(_batch);

		_drawTextHelperMock.Verify(m => m.DrawAlignedText(
			_batch,
			expectedHeaderX,
			expectedY,
			"translation-fse.forecast.menu.header.item",
			DrawTextHelper.DrawTextAlignment.Middle,
			DrawTextHelper.DrawTextAlignment.Middle,
			false
		));

		_drawTextHelperMock.Verify(m => m.DrawAlignedText(
			_batch,
			expectedSellPriceX,
			expectedY,
			"translation-fse.forecast.menu.header.sellPrice",
			DrawTextHelper.DrawTextAlignment.Middle,
			DrawTextHelper.DrawTextAlignment.Middle,
			false
		));

		_drawTextHelperMock.Verify(m => m.DrawAlignedText(
			_batch,
			expectedPerDayX,
			expectedY,
			"translation-fse.forecast.menu.header.sellPrice",
			DrawTextHelper.DrawTextAlignment.Middle,
			DrawTextHelper.DrawTextAlignment.Start,
			false
		));

		_drawTextHelperMock.Verify(m => m.DrawAlignedText(
			_batch,
			expectedPerDayX,
			expectedY,
			"translation-fse.forecast.menu.header.perDay",
			DrawTextHelper.DrawTextAlignment.Middle,
			DrawTextHelper.DrawTextAlignment.End,
			false
		));

		_drawTextHelperMock.Verify(m => m.DrawAlignedText(
			_batch,
			expectedSupplyX,
			expectedY,
			"translation-fse.forecast.menu.header.supply",
			DrawTextHelper.DrawTextAlignment.Middle,
			DrawTextHelper.DrawTextAlignment.Start,
			false
		));

		_drawTextHelperMock.Verify(m => m.DrawAlignedText(
			_batch,
			expectedSupplyX,
			expectedY,
			"translation-fse.forecast.menu.header.supplyDescriptor",
			DrawTextHelper.DrawTextAlignment.Middle,
			DrawTextHelper.DrawTextAlignment.End,
			false
		));
	}

	[TestCase(1080, 620, 150, 203, 150, 155)]
	[TestCase(10800, 6200, 4490, 2253, 4490, 2205)]
	public void ShouldDrawCategoryDropDown
	(
		int screenWidth,
		int screenHeight,
		int expectedX,
		int expectedY,
		int expectedLabelX,
		int expectedLabelY
	)
	{
		Game1.uiViewport.Width = screenWidth;
		Game1.uiViewport.Height = screenHeight;

		_menu.draw(_batch);

		var dropDown = HarmonyOptionsDropDown.DrawCalls.First().Key;

		Assert.Multiple(() =>
		{
			Assert.That(dropDown.dropDownOptions, Is.EqualTo(new[] { int.MinValue.ToString(), "1", "2", "3" }));
			Assert.That(dropDown.dropDownDisplayOptions, Is.EqualTo(new[]
			{
				"translation-fse.forecast.menu.allCategory",
				"Category1",
				"Category2",
				"Category3",
			}));

			Assert.That(dropDown.bounds.X, Is.EqualTo(expectedX));
			Assert.That(dropDown.bounds.Y, Is.EqualTo(expectedY));
		});

		_drawTextHelperMock.Verify(m => m.DrawAlignedText(
			_batch,
			expectedLabelX,
			expectedLabelY,
			"translation-fse.forecast.menu.header.category",
			DrawTextHelper.DrawTextAlignment.Middle,
			DrawTextHelper.DrawTextAlignment.End,
			false
		));
	}

	[TestCase(1080, 620, 930, 203, 930, 155)]
	[TestCase(10800, 6200, 6310, 2253, 6310, 2205)]
	public void ShouldDrawSortingDropDown
	(
		int screenWidth,
		int screenHeight,
		int expectedX,
		int expectedY,
		int expectedLabelX,
		int expectedLabelY
	)
	{
		Game1.uiViewport.Width = screenWidth;
		Game1.uiViewport.Height = screenHeight;

		_menu.draw(_batch);

		var dropDown = HarmonyOptionsDropDown.DrawCalls.ToArray()[1].Key;

		Assert.Multiple(() =>
		{
			Assert.That(dropDown.dropDownOptions, Is.EqualTo(new[]
			{
				"None",
				"Name",
				"Supply",
				"DailyChange",
				"MarketPrice",
				"MarketPricePerDay",
			}));
			Assert.That(dropDown.dropDownDisplayOptions, Is.EqualTo(new[]
			{
				"translation-fse.forecast.menu.sort.none",
				"translation-fse.forecast.menu.sort.name",
				"translation-fse.forecast.menu.sort.supply",
				"translation-fse.forecast.menu.sort.delta",
				"translation-fse.forecast.menu.sort.marketPrice",
				"translation-fse.forecast.menu.sort.marketPricePerDay",
			}));

			Assert.That(dropDown.bounds.X, Is.EqualTo(expectedX));
			Assert.That(dropDown.bounds.Y, Is.EqualTo(expectedY));
		});

		_drawTextHelperMock.Verify(m => m.DrawAlignedText(
			_batch,
			expectedLabelX,
			expectedLabelY,
			"translation-fse.forecast.menu.header.sortBy",
			DrawTextHelper.DrawTextAlignment.Middle,
			DrawTextHelper.DrawTextAlignment.End,
			false
		));
	}

	[TestCase(Season.Spring, 1080, 620, 531, 539, 543, 549, 203, true, false, false, false)]
	[TestCase(Season.Spring, 10800, 6200, 5391, 5399, 5403, 5409, 2253, true, false, false, false)]
	[TestCase(Season.Summer, 1080, 620, 531, 539, 543, 549, 203, false, true, false, false)]
	[TestCase(Season.Summer, 10800, 6200, 5391, 5399, 5403, 5409, 2253, false, true, false, false)]
	[TestCase(Season.Fall, 1080, 620, 531, 539, 543, 549, 203, false, false, true, false)]
	[TestCase(Season.Fall, 10800, 6200, 5391, 5399, 5403, 5409, 2253, false, false, true, false)]
	[TestCase(Season.Winter, 1080, 620, 531, 539, 543, 549, 203, false, false, false, true)]
	[TestCase(Season.Winter, 10800, 6200, 5391, 5399, 5403, 5409, 2253, false, false, false, true)]
	public void ShouldDrawSeasonCheckbox
	(
		Season season,
		int screenWidth,
		int screenHeight,
		int checkbox1X,
		int checkbox2X,
		int checkbox3X,
		int checkbox4X,
		int checkboxY,
		bool checkbox1Checked,
		bool checkbox2Checked,
		bool checkbox3Checked,
		bool checkbox4Checked
	)
	{
		Game1.uiViewport.Width = screenWidth;
		Game1.uiViewport.Height = screenHeight;
		Game1.season = season;

		_menu = new ForecastMenu(_helperMock.Object, _economyServiceMock.Object, _monitorMock.Object,
			_drawTextHelperMock.Object);

		_menu.draw(_batch);

		var checkboxes = HarmonyOptionsCheckbox.DrawCalls.ToArray();

		var check1 = checkboxes[0].Key;
		var check2 = checkboxes[1].Key;
		var check3 = checkboxes[2].Key;
		var check4 = checkboxes[3].Key;
		Assert.Multiple(() =>
		{
			Assert.That(check1.isChecked, Is.EqualTo(checkbox1Checked), "Checkbox1 checked state is not correct");
			Assert.That(check2.isChecked, Is.EqualTo(checkbox2Checked), "Checkbox2 checked state is not correct");
			Assert.That(check3.isChecked, Is.EqualTo(checkbox3Checked), "Checkbox3 checked state is not correct");
			Assert.That(check4.isChecked, Is.EqualTo(checkbox4Checked), "Checkbox4 checked state is not correct");
			Assert.That(check1.bounds.X, Is.EqualTo(checkbox1X), "Checkbox 1 X position does not match expectation");
			Assert.That(check2.bounds.X, Is.EqualTo(checkbox2X), "Checkbox 2 X position does not match expectation");
			Assert.That(check3.bounds.X, Is.EqualTo(checkbox3X), "Checkbox 3 X position does not match expectation");
			Assert.That(check4.bounds.X, Is.EqualTo(checkbox4X), "Checkbox 4 X position does not match expectation");
			Assert.That(check1.bounds.Y, Is.EqualTo(checkboxY), "Checkbox 1 Y doesn't match expected");
			Assert.That(check2.bounds.Y, Is.EqualTo(checkboxY), "Checkbox 2 Y doesn't match expected");
			Assert.That(check3.bounds.Y, Is.EqualTo(checkboxY), "Checkbox 3 Y doesn't match expected");
			Assert.That(check4.bounds.Y, Is.EqualTo(checkboxY), "Checkbox 4 Y doesn't match expected");
		});
	}

	[TestCase(1080, 620, 944, 42)]
	[TestCase(10800, 6200, 5439, 2581)]
	public void ShouldDrawExitButton(int screenWidth, int screenHeight, int x, int y)
	{
		Game1.uiViewport.Width = screenWidth;
		Game1.uiViewport.Height = screenHeight;

		_menu.draw(_batch);

		var exitButton = HarmonyClickableTextureComponent.DrawCalls.ToArray()[4].Key;
		Assert.Multiple(() =>
		{
			Assert.That(exitButton.bounds.X, Is.EqualTo(x));
			Assert.That(exitButton.bounds.Y, Is.EqualTo(y));
		});
	}

	[TestCase(1, 10, 100, 1000, 10, 92, 25, 229, 0, 0, 1, 0, 1003, 204, 582, 802)]
	[TestCase(1, 20, 100, 1000, 10, 92, 25, 229, 0, 0, 2, 0, 993, 204, 582, 802)]
	[TestCase(1, 30, 100, 1000, 10, 92, 25, 229, 0, 0, 3, 0, 983, 204, 582, 802)]
	[TestCase(1, -10, 100, 1000, 10, 92, 25, 229, 0, 1, 0, 993, 0, 204, 582, 802)]
	[TestCase(1, -20, 100, 1000, 10, 92, 25, 229, 0, 2, 0, 1003, 0, 204, 582, 802)]
	[TestCase(1, -30, 100, 1000, 10, 92, 25, 229, 0, 3, 0, 1013, 0, 204, 582, 802)]
	[TestCase(1, 10, 100, 100, -1, 92, 25, 229, 0, 0, 1, 0, 1003, 204, 582, 802)]
	[TestCase(1, 10, 500, 1000, 10, 460, 127, 127, 0, 0, 1, 0, 1371, 204, 582, 802)]
	[TestCase(2, 10, 100, 1000, 10, 92, 25, 229, 0, 0, 1, 0, 1003, 204, 582, 802)]
	public void ShouldDrawRow
	(
		int expectedRows,
		int delta,
		int supply,
		int sellPrice,
		int sellPricePerDay,
		int expectedBarWidth,
		int expectedBarColorRed,
		int expectedBarColorGreen,
		int expectedBarColorBlue,
		int expectedLeftArrowCalls,
		int expectedRightArrowCalls,
		int expectedLeftArrowLocation,
		int expectedRightArrowLocation,
		int expectedNameLocation,
		int expectedPriceLocation,
		int expectedPerDayLocation
	)
	{
		ConfigModel.Instance.MinDelta = -1000;
		Game1.uiViewport.Width = 2000;
		Game1.uiViewport.Height = 620 * expectedRows;

		var models = new List<ItemModel>
		{
			new() { DailyDelta = delta, ObjectId = sellPrice.ToString(), Supply = supply },
			new() { DailyDelta = delta, ObjectId = sellPrice.ToString(), Supply = supply },
		};

		_economyServiceMock.Setup(m => m.GetPricePerDay(models[0])).Returns(sellPricePerDay);
		_economyServiceMock.Setup(m => m.GetPricePerDay(models[1])).Returns(sellPricePerDay);

		_economyServiceMock.Setup(m => m.GetItemsForCategory(1)).Returns(models.ToArray);
		_menu = new ForecastMenu(_helperMock.Object, _economyServiceMock.Object, _monitorMock.Object,
			_drawTextHelperMock.Object);
		Game1.staminaRect = new Texture2D(null, 0, 0);

		_menu.draw(_batch);

		var dropDown = HarmonyOptionsDropDown.DrawCalls.First().Key;
		dropDown.selectedOption = 1;

		dropDown.bounds.Width = 10;
		dropDown.bounds.Height = 10;

		_menu.receiveLeftClick(dropDown.bounds.Center.X, dropDown.bounds.Center.Y);
		_menu.releaseLeftClick(dropDown.bounds.Center.X, dropDown.bounds.Center.Y);

		HarmonyObject.DrawInMenuCalls.Clear();
		HarmonyIClickableMenu.DrawHoriztonalPartitionCalls.Clear();
		HarmonySpriteBatch.DrawCalls.Clear();
		_drawTextHelperMock.Invocations.Clear();

		_menu.draw(_batch);

		var drawIconLocation = HarmonyObject.DrawInMenuCalls[models[0].GetObjectInstance()].First();
		Assert.Multiple(() =>
		{
			Assert.That(drawIconLocation.X, Is.EqualTo(140f));
			Assert.That(drawIconLocation.Y, Is.EqualTo(415f));
		});

		Assert.That(HarmonyIClickableMenu.DrawHoriztonalPartitionCalls[_batch], Has.Count.EqualTo(1 + expectedRows));

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		var supplyBarCalls = HarmonySpriteBatch.DrawCalls[_batch].Where(b => b.texture == Game1.staminaRect).ToArray();

		//doesn't seem that useful to unit test graphics being drawn precisely. Reconsider if bugs arise.
		Assert.That(supplyBarCalls, Has.Length.GreaterThanOrEqualTo(18 * expectedRows));

		var negativeDeltaArrows = HarmonyClickableTextureComponent.DrawCalls.Keys.FirstOrDefault(c => c.name == "left-arrow");

		if (expectedLeftArrowCalls != 0)
		{
			Assert.Multiple(() =>
			{
				Assert.That(HarmonyClickableTextureComponent.DrawCalls[negativeDeltaArrows!], Is.EqualTo(expectedLeftArrowCalls));
				Assert.That(negativeDeltaArrows!.bounds.X, Is.EqualTo(expectedLeftArrowLocation));
				Assert.That(negativeDeltaArrows.bounds.Y, Is.EqualTo(411));
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
				Assert.That(positiveDeltaArrows.bounds.Y, Is.EqualTo(411));
			});
		}
		else
		{
			Assert.That(positiveDeltaArrows, Is.Null);
		}

		if (expectedRows > 1)
		{
			var negativeDeltaArrow2 = HarmonyClickableTextureComponent.DrawCalls.Keys.Where(c => c.name == "left-arrow").Skip(1)
				.FirstOrDefault();

			if (expectedLeftArrowCalls != 0)
			{
				Assert.Multiple(() =>
				{
					Assert.That(HarmonyClickableTextureComponent.DrawCalls[negativeDeltaArrow2!], Is.EqualTo(expectedLeftArrowCalls));
					Assert.That(negativeDeltaArrow2!.bounds.X, Is.EqualTo(expectedLeftArrowLocation));
					Assert.That(negativeDeltaArrow2.bounds.Y, Is.EqualTo(510));
				});
			}
			else
			{
				Assert.That(negativeDeltaArrow2, Is.Null);
			}

			var positiveDeltaArrow2 = HarmonyClickableTextureComponent.DrawCalls.Keys.Where(c => c.name == "right-arrow").Skip(1)
				.FirstOrDefault();

			if (expectedRightArrowCalls != 0)
			{
				Assert.Multiple(() =>
				{
					Assert.That(HarmonyClickableTextureComponent.DrawCalls[positiveDeltaArrow2!], Is.EqualTo(expectedRightArrowCalls));
					Assert.That(positiveDeltaArrow2!.bounds.X, Is.EqualTo(expectedRightArrowLocation));
					Assert.That(positiveDeltaArrow2.bounds.Y, Is.EqualTo(531));
				});
			}
			else
			{
				Assert.That(positiveDeltaArrows, Is.Null);
			}
		}

		_drawTextHelperMock.Verify(m => m.DrawAlignedText
			(
				_batch,
				expectedNameLocation,
				445,
				$"display-{sellPrice}",
				DrawTextHelper.DrawTextAlignment.Start,
				DrawTextHelper.DrawTextAlignment.Middle,
				false
			)
		);

		_drawTextHelperMock.Verify(m => m.DrawAlignedText
			(
				_batch,
				expectedPriceLocation,
				445,
				sellPrice.ToString(),
				DrawTextHelper.DrawTextAlignment.Middle,
				DrawTextHelper.DrawTextAlignment.Middle,
				false
			)
		);

		_drawTextHelperMock.Verify(m => m.DrawAlignedText
			(
				_batch,
				expectedPerDayLocation,
				445,
				sellPricePerDay != -1 ? sellPricePerDay.ToString() : "---",
				DrawTextHelper.DrawTextAlignment.Middle,
				DrawTextHelper.DrawTextAlignment.Middle,
				false
			)
		);

		_drawTextHelperMock.Verify(m => m.DrawAlignedText
			(
				_batch,
				expectedNameLocation,
				565,
				$"display-{sellPrice}",
				DrawTextHelper.DrawTextAlignment.Start,
				DrawTextHelper.DrawTextAlignment.Middle,
				false
			), Times.Exactly(expectedRows > 1 ? 1 : 0)
		);

		_drawTextHelperMock.Verify(m => m.DrawAlignedText
			(
				_batch,
				expectedPriceLocation,
				565,
				sellPrice.ToString(),
				DrawTextHelper.DrawTextAlignment.Middle,
				DrawTextHelper.DrawTextAlignment.Middle,
				false
			), Times.Exactly(expectedRows > 1 ? 1 : 0)
		);

		_drawTextHelperMock.Verify(m => m.DrawAlignedText
			(
				_batch,
				expectedPerDayLocation,
				565,
				sellPricePerDay != -1 ? sellPricePerDay.ToString() : "---",
				DrawTextHelper.DrawTextAlignment.Middle,
				DrawTextHelper.DrawTextAlignment.Middle,
				false
			), Times.Exactly(expectedRows > 1 ? 1 : 0)
		);
	}

	[TestCase(0, 6)]
	[TestCase(1, 2)]
	[TestCase(2, 3)]
	[TestCase(3, 1)]
	public void ShouldDrawCorrectNumberOfRowsForCategory(int selectedOption, int expectedRows)
	{
		ConfigModel.Instance.MinDelta = -1000;
		Game1.uiViewport.Width = 2000;
		Game1.uiViewport.Height = 6200;

		_menu = new ForecastMenu(_helperMock.Object, _economyServiceMock.Object, _monitorMock.Object,
			_drawTextHelperMock.Object);

		_menu.draw(_batch);

		var dropDown = HarmonyOptionsDropDown.DrawCalls.First().Key;
		dropDown.selectedOption = selectedOption;

		dropDown.bounds.Width = 10;
		dropDown.bounds.Height = 10;

		_menu.receiveLeftClick(dropDown.bounds.Center.X, dropDown.bounds.Center.Y);
		_menu.releaseLeftClick(dropDown.bounds.Center.X, dropDown.bounds.Center.Y);

		HarmonyIClickableMenu.DrawHoriztonalPartitionCalls.Clear();
		_menu.draw(_batch);

		Assert.That(HarmonyIClickableMenu.DrawHoriztonalPartitionCalls[_batch], Has.Count.EqualTo(expectedRows + 2));
	}
}