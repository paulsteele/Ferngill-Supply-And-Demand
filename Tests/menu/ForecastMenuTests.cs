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
	private ForecastMenu _menu;
	private Mock<IModHelper> _helperMock;
	private Mock<IEconomyService> _economyServiceMock;
	private Mock<IMonitor> _monitorMock;
	private Mock<IDrawTextHelper> _drawTextHelperMock;

	private SpriteBatch _batch;

	[SetUp]
	public override void Setup()
	{
		base.Setup();
		
		_helperMock = new Mock<IModHelper>();
		_economyServiceMock = new Mock<IEconomyService>();
		_monitorMock = new Mock<IMonitor>();
		_drawTextHelperMock = new Mock<IDrawTextHelper>();

		_helperMock.Setup(m => m.Translation).Returns(new MockTranslationHelper());

		ConfigModel.Instance = new ConfigModel()
		{
			MinDelta = 0,
			MaxDelta = 1000,
		};

		_economyServiceMock.Setup(x => x.Loaded).Returns(true);
		_economyServiceMock.Setup(x => x.GetCategories()).Returns(new Dictionary<int, string>()
		{
			{ 1, "Category1" },
			{ 2, "Category2" },
			{ 3, "Category3" },
		});
		_economyServiceMock.Setup(m => m.GetItemsForCategory(1)).Returns(
			[
				new ItemModel {ObjectId = "1", Supply = 100, DailyDelta = 100},
				new ItemModel {ObjectId = "2", Supply = 200, DailyDelta = 200},
			]
		);
		_economyServiceMock.Setup(m => m.GetItemsForCategory(2)).Returns(
			[
				new ItemModel {ObjectId = "3", Supply = 300, DailyDelta = 300},
				new ItemModel {ObjectId = "4", Supply = 400, DailyDelta = 400},
				new ItemModel {ObjectId = "5", Supply = 500, DailyDelta = 500},
			]
		);
		_economyServiceMock.Setup(m => m.GetItemsForCategory(3)).Returns(
			[
				new ItemModel {ObjectId = "6", Supply = 600, DailyDelta = 600},
			]
		);

		_economyServiceMock.Setup(m => m.ItemValidForSeason(It.IsAny<ItemModel>(), It.IsAny<Seasons>())).Returns(true);

		HarmonyGame.GetOptionsResult = new Options();
		Game1.graphics = new GraphicsDeviceManager(null);

		_batch = new SpriteBatch(null, 0);
		_menu = new ForecastMenu(_helperMock.Object, _economyServiceMock.Object, _monitorMock.Object, _drawTextHelperMock.Object);
	}

	[TearDown]
	public void Teardown()
	{
		_batch.Dispose();
	}

	[TestCase(1080, 620, 0, 0, 0, 0)]
	public void ShouldSetupPositionAndSize
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
		
		Assert.Multiple(() =>
		{
			Assert.That(_menu.width, Is.EqualTo(expectedWidth), "Menu width does not match expectation");
			Assert.That(_menu.height, Is.EqualTo(expectedHeight), "Menu height does not match expectation");
			Assert.That(_menu.xPositionOnScreen, Is.EqualTo(expectedX), "Menu x-coordinate does not match expectation");
			Assert.That(_menu.yPositionOnScreen, Is.EqualTo(expectedY), "Menu y-coordinate does not match expectation");
		});
	}
}