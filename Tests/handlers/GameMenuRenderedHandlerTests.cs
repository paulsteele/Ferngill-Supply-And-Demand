using fse.core.handlers;
using fse.core.menu;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
	private Mock<IModContentHelper> _mockModContent;
	private Mock<IModRegistry> _mockModRegistry;
	private Mock<IInputHelper> _mockInputHelper;
	private Mock<ITooltipMenu> _mockTooltipService;

	private GameMenuLoadedHandler _gameMenuLoadedHandler;
	private GameMenu _gameMenu;
	private SpriteBatch _batch;
	private Texture2D _assetTexture;

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
		_mockModContent = new Mock<IModContentHelper>();
		_mockModRegistry = new Mock<IModRegistry>();
		_mockInputHelper = new Mock<IInputHelper>();
		_mockTooltipService = new Mock<ITooltipMenu>();
		
		var mockEvents = new Mock<IModEvents>();
		mockEvents.Setup(m => m.Display).Returns(_mockDisplayEvents);
		mockEvents.Setup(m => m.Input).Returns(_mockInputEvents);
		_mockModHelper.Setup(m => m.Events).Returns(mockEvents.Object);

		_mockModHelper.Setup(m => m.Translation).Returns(new MockTranslationHelper());
		_mockModHelper.Setup(m => m.ModContent).Returns(_mockModContent.Object);
		_mockModHelper.Setup(m => m.ModRegistry).Returns(_mockModRegistry.Object);
		_mockModHelper.Setup(m => m.Input).Returns(_mockInputHelper.Object);
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
		
		_batch = new SpriteBatch(null, 0);
		_assetTexture = new Texture2D(null, 0, 0);
		
		ConfigModel.Instance = new ConfigModel
		{
			EnableMenuTab = true,
		};
		Game1.spriteBatch = _batch;

		_mockInputHelper.Setup(m => m.GetCursorPosition()).Returns(_mockCursor.Object);
		_mockCursor.SetupGet(m => m.ScreenPixels).Returns(new Vector2(32, 32));
		_mockForecastMenuService.Setup(m => m.CreateMenu(It.IsAny<Action>())).Returns(_mockForecastMenu.Object);
		_mockModContent.Setup(m => m.Load<Texture2D>(It.IsAny<string>())).Returns(_assetTexture);

		_gameMenuLoadedHandler = new GameMenuLoadedHandler(_mockModHelper.Object, _mockMonitor.Object, _mockForecastMenuService.Object, _mockTooltipService.Object);
		_gameMenuLoadedHandler.Register();
	}

	[TearDown]
	public void Teardown()
	{
		_batch.Dispose();
		_assetTexture.Dispose();
	}

	[Test]
	public void ShouldCreateForecastMenuWhenAreaTapped()
	{
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
		Assert.That(Game1.activeClickableMenu, Is.EqualTo(_mockForecastMenu.Object));
	}
	
	[Test]
	public void ShouldNotCreateForecastMenuWhenNotInGameMenu()
	{
		HarmonyGame.GetActiveClickableMenuResult = null!;
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
		Assert.That(Game1.activeClickableMenu, Is.Not.EqualTo(_mockForecastMenu.Object));
	}
	
	[Test]
	public void ShouldNotCreateForecastMenuWhenConfigDisabled()
	{
		ConfigModel.Instance.EnableMenuTab = false;
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
		Assert.That(Game1.activeClickableMenu, Is.Not.EqualTo(_mockForecastMenu.Object));
	}
	
	[Test]
	public void ShouldNotCreateForecastMenuWhenWrongButtonPressed()
	{
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseRight, _mockCursor.Object));
		
		Assert.That(Game1.activeClickableMenu, Is.Not.EqualTo(_mockForecastMenu.Object));
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

		Assert.That(Game1.activeClickableMenu, expectedToOpen ? Is.EqualTo(_mockForecastMenu.Object) : Is.Not.EqualTo(_mockForecastMenu.Object));
	}
	
	[Test]
	public void ShouldNotCreateForecastMenuWhenForecastMenuIsAlreadyOpen()
	{
		_gameMenu.pages[0] = _mockForecastMenu.Object;
		_mockInputEvents.InvokeButtonPressed(HarmonyButtonPressedEventArgs.CreateButtonPressedEventArgs(SButton.MouseLeft, _mockCursor.Object));
		
		Assert.That(Game1.activeClickableMenu, Is.Not.EqualTo(_mockForecastMenu.Object));
	}

	[TestCase(0, 0, 0, true, 704, 16)]
	[TestCase(0, 0, 0, false, 774, 16)]
	[TestCase(0, 0, 1, true, 705, 16)]
	[TestCase(0, 0, 1, false, 775, 16)]
	[TestCase(50, 50, 0, true, 754, 66)]
	[TestCase(50, 50, 50, true, 804, 66)]
	public void ShouldDrawTabInCorrectPosition
	(
		int menuX,
		int menuY,
		int offset,
		bool isExitPageLastTab,
		int expectedX,
		int expectedY
	)
	{
		_gameMenu.xPositionOnScreen = menuX;
		_gameMenu.yPositionOnScreen = menuY;
		ConfigModel.Instance.MenuTabOffset = offset;

		if (isExitPageLastTab)
		{
			var exitPage = new ExitPage(0, 0, 0, 0);
			_gameMenu.pages.Add(exitPage);
		}	
		_mockDisplayEvents.InvokeRenderedActiveMenu(new RenderedActiveMenuEventArgs());

		var calls = HarmonySpriteBatch.DrawCalls[_batch];
		
		Assert.Multiple(() =>
		{ 
			Assert.That(calls[0].texture, Is.EqualTo(_assetTexture)); 
			Assert.That(calls[0].sourceRectangle, Is.EqualTo(new Rectangle(0, 0, 16, 16))); 
			Assert.That(calls[0].color, Is.EqualTo(Color.White)); 
			Assert.That(calls[0].rotation, Is.EqualTo(0)); 
			Assert.That(calls[0].origin, Is.EqualTo(Vector2.Zero)); 
			Assert.That(calls[0].scale, Is.EqualTo(4f)); 
			Assert.That(calls[0].effects, Is.EqualTo(SpriteEffects.None)); 
			Assert.That(calls[0].layerDepth, Is.EqualTo(0.0001f));
			Assert.That(HarmonyIClickableMenu.DrawMouseCalls[_batch], Is.EqualTo(1));
			Assert.That(HarmonyIClickableMenu.DrawHoverTextCalls, Has.Count.EqualTo(0));
		});

		var component = _gameMenuLoadedHandler.Tab;
		
		Assert.Multiple(() =>
		{
			Assert.That(component, Is.Not.Null);
			Assert.That(component.bounds.X, Is.EqualTo(expectedX), ".x");
			Assert.That(component.bounds.Y, Is.EqualTo(expectedY), ".y");
		});
	}

	[Test]
	public void ShouldNotDisplayWhenConfigDisabled()
	{
		ConfigModel.Instance.EnableMenuTab = false;
		_mockDisplayEvents.InvokeRenderedActiveMenu(new RenderedActiveMenuEventArgs());

		var calls = HarmonySpriteBatch.DrawCalls;
		Assert.That(calls, Has.Count.EqualTo(0));
	}
	
	[Test]
	public void ShouldNotDisplayWhenActiveMenuIsNotGameMenu()
	{
		HarmonyGame.GetActiveClickableMenuResult = new Mock<IClickableMenu>().Object;
		_mockDisplayEvents.InvokeRenderedActiveMenu(new RenderedActiveMenuEventArgs());

		var calls = HarmonySpriteBatch.DrawCalls;
		Assert.That(calls, Has.Count.EqualTo(0));
	}
	
	[Test]
	public void ShouldNotDisplayWhenActiveTabIsMap()
	{
		_gameMenu.pages[0] = new MapPage(0, 0, 0, 0);
		_mockDisplayEvents.InvokeRenderedActiveMenu(new RenderedActiveMenuEventArgs());

		var calls = HarmonySpriteBatch.DrawCalls;
		Assert.That(calls, Has.Count.EqualTo(0));
	}
	
	[Test]
	public void ShouldDisplayWhenActiveTabIsCollectionsPageWhenLetterNotOpened()
	{
		_gameMenu.pages[0] = new CollectionsPage(0, 0, 0, 0);
		_mockDisplayEvents.InvokeRenderedActiveMenu(new RenderedActiveMenuEventArgs());

		var calls = HarmonySpriteBatch.DrawCalls;
		Assert.That(calls, Has.Count.EqualTo(1));
	}
	
	[Test]
	public void ShouldNotDisplayWhenActiveTabIsCollectionsPageWhenLetterOpened()
	{
		_gameMenu.pages[0] = new CollectionsPage(0, 0, 0, 0)
		{
			letterviewerSubMenu = new LetterViewerMenu(0)
		};
		_mockDisplayEvents.InvokeRenderedActiveMenu(new RenderedActiveMenuEventArgs());

		var calls = HarmonySpriteBatch.DrawCalls;
		Assert.That(calls, Has.Count.EqualTo(0));
	}
	
	[Test]
	public void ShouldRedrawHoverTextIfItPreviouslyWasDrawn()
	{
		_gameMenu.hoverText = "test text";
		_mockDisplayEvents.InvokeRenderedActiveMenu(new RenderedActiveMenuEventArgs());

		var hoverDrawCalls = HarmonyIClickableMenu.DrawHoverTextCalls;
		
		Assert.That(hoverDrawCalls, Has.Count.EqualTo(1));

		var call = HarmonyIClickableMenu.DrawHoverTextCalls[_batch];
		
		Assert.That(call, Is.EqualTo("test text"));
	}
	
	[Test]
	public void ShouldOverwriteHoverTextIfItPreviouslyWasDrawnButAlsoHoveringTab()
	{
		_gameMenu.hoverText = "test text";
		
		_mockCursor.SetupGet(m => m.ScreenPixels).Returns(new Vector2(780, 32));
		
		_mockDisplayEvents.InvokeRenderedActiveMenu(new RenderedActiveMenuEventArgs());

		var hoverDrawCalls = HarmonyIClickableMenu.DrawHoverTextCalls;
		
		Assert.That(hoverDrawCalls, Has.Count.EqualTo(1));

		var call = HarmonyIClickableMenu.DrawHoverTextCalls[_batch];
		
		Assert.That(call, Is.EqualTo("translation-fse.forecast.menu.tab.title"));
	}
	
	[Test]
	public void ShouldNotDrawHoverTextIfNoneExists()
	{
		_mockDisplayEvents.InvokeRenderedActiveMenu(new RenderedActiveMenuEventArgs());

		var hoverDrawCalls = HarmonyIClickableMenu.DrawHoverTextCalls;
		
		Assert.That(hoverDrawCalls, Has.Count.EqualTo(0));
	}
}
