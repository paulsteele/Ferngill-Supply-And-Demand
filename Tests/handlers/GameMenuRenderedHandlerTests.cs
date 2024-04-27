using fse.core.extensions;
using fse.core.handlers;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Moq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Tests.HarmonyMocks;
using Tests.Mocks;

namespace Tests.handlers;

public class GameMenuRenderedHandlerTests : HarmonyTestBase
{
	private Mock<IModHelper> _mockModHelper;
	private MockDisplayEvents _mockDisplayEvents;
	private MockInputEvents _mockInputEvents;
	private Mock<IMonitor> _mockMonitor;
	private Mock<IEconomyService> _mockEconomyService;
	private Mock<ICursorPosition> _mockCursor;

	private GameMenuLoadedHandler _gameMenuLoadedHandler;

	[SetUp]
	public override void Setup()
	{
		base.Setup();

		_mockMonitor = new Mock<IMonitor>();
		_mockEconomyService = new Mock<IEconomyService>();
		_mockModHelper = new Mock<IModHelper>();
		_mockDisplayEvents = new MockDisplayEvents();
		_mockInputEvents = new MockInputEvents();
		_mockCursor = new Mock<ICursorPosition>();
		
		var mockEvents = new Mock<IModEvents>();
		mockEvents.Setup(m => m.Display).Returns(_mockDisplayEvents);
		mockEvents.Setup(m => m.Input).Returns(_mockInputEvents);
		_mockModHelper.Setup(m => m.Events).Returns(mockEvents.Object);

		_mockModHelper.Setup(m => m.Translation).Returns(new MockTranslationHelper());
		var gameMenu = new GameMenu
		{
			currentTab = 0,
			pages =
			[
				new Mock<IClickableMenu>().Object,
			],
		};


		HarmonyGame.GetActiveClickableMenuResult = gameMenu;
		HarmonyGame.GetOptionsResult = new Options();
		
		ConfigModel.Instance = new ConfigModel
		{
			EnableMenuTab = true,
		};

		_mockCursor.SetupGet(m => m.ScreenPixels).Returns(new Vector2(32, 32));

		_gameMenuLoadedHandler = new GameMenuLoadedHandler(_mockModHelper.Object, _mockMonitor.Object, _mockEconomyService.Object);
		_gameMenuLoadedHandler.Register();
	}

	[Test]
	public void ShouldCreateForecastMenuWhenAreaTapped()
	{
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
	}
}
