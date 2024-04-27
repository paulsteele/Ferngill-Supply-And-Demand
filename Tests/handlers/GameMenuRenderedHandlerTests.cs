using fse.core.extensions;
using fse.core.handlers;
using fse.core.menu;
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
	private Mock<ICursorPosition> _mockCursor;
	private Mock<IForecastMenuService> _mockForecastMenuService;
	private Mock<AbstractForecastMenu> _mockForecastMenu;

	private GameMenuLoadedHandler _gameMenuLoadedHandler;
	private GameMenu _gameMenu;

	[SetUp]
	public override void Setup()
	{
		base.Setup();

		_mockMonitor = new Mock<IMonitor>();
		_mockModHelper = new Mock<IModHelper>();
		_mockDisplayEvents = new MockDisplayEvents();
		_mockInputEvents = new MockInputEvents();
		_mockCursor = new Mock<ICursorPosition>();
		_mockForecastMenuService = new Mock<IForecastMenuService>();
		_mockForecastMenu = new Mock<AbstractForecastMenu>();
		
		var mockEvents = new Mock<IModEvents>();
		mockEvents.Setup(m => m.Display).Returns(_mockDisplayEvents);
		mockEvents.Setup(m => m.Input).Returns(_mockInputEvents);
		_mockModHelper.Setup(m => m.Events).Returns(mockEvents.Object);

		_mockModHelper.Setup(m => m.Translation).Returns(new MockTranslationHelper());
		_gameMenu = new GameMenu
		{
			currentTab = 0,
			pages =
			[
				new Mock<IClickableMenu>().Object,
			],
		};

		HarmonyGame.GetActiveClickableMenuResult = _gameMenu;
		HarmonyGame.GetOptionsResult = new Options();
		
		ConfigModel.Instance = new ConfigModel
		{
			EnableMenuTab = true,
		};

		_mockCursor.SetupGet(m => m.ScreenPixels).Returns(new Vector2(32, 32));
		_mockForecastMenuService.Setup(m => m.CreateMenu()).Returns(_mockForecastMenu.Object);

		_gameMenuLoadedHandler = new GameMenuLoadedHandler(_mockModHelper.Object, _mockMonitor.Object, _mockForecastMenuService.Object);
		_gameMenuLoadedHandler.Register();
	}

	[Test]
	public void ShouldCreateForecastMenuWhenAreaTapped()
	{
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
		_mockForecastMenu.Verify(m => m.TakeOverMenuTab(_gameMenu), Times.Once);
	}
	
	[Test]
	public void ShouldNotCreateForecastMenuWhenNotInGameMenu()
	{
		HarmonyGame.GetActiveClickableMenuResult = null!;
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
		_mockForecastMenu.Verify(m => m.TakeOverMenuTab(_gameMenu), Times.Never);
	}
	
	[Test]
	public void ShouldNotCreateForecastMenuWhenConfigDisabled()
	{
		ConfigModel.Instance.EnableMenuTab = false;
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
		_mockForecastMenu.Verify(m => m.TakeOverMenuTab(_gameMenu), Times.Never);
	}
	
	[Test]
	public void ShouldNotCreateForecastMenuWhenWrongButtonPressed()
	{
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseRight, _mockCursor.Object));
		
		_mockForecastMenu.Verify(m => m.TakeOverMenuTab(_gameMenu), Times.Never);
	}
	
	[TestCase(32, 32, 1, 1, true)]
	[TestCase(65, 32, 1, 1, false)]
	[TestCase(32, 65, 1, 1, false)]
	[TestCase(65, 65, 2, 1, true)]
	[TestCase(33, 33, 1, 2, false)]
	[TestCase(31, 31, 1, 2, true)]
	public void ShouldAccountForScalingWhenTappingIndicator
	(
		float xClick,
		float yClick,
		float uiScale,
		float zoom,
		bool expectedToOpen
	)
	{
		_mockCursor.SetupGet(m => m.ScreenPixels).Returns(new Vector2(xClick, yClick));
		HarmonyOptions.GetUiScaleResult = uiScale;
		HarmonyOptions.GetZoomLevelResult = zoom;
		
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
		_mockForecastMenu.Verify(m => m.TakeOverMenuTab(_gameMenu), Times.Exactly(expectedToOpen ? 1 : 0));
	}
	
	[Test]
	public void ShouldNotCreateForecastMenuWhenForecastMenuIsAlreadyOpen()
	{
		_gameMenu.pages[0] = _mockForecastMenu.Object;
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
		_mockForecastMenu.Verify(m => m.TakeOverMenuTab(_gameMenu), Times.Never);
	}
}
