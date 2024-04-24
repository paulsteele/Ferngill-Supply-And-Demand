using fse.core.handlers;
using fse.core.services;
using Moq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Tests.Mocks;

namespace Tests.handlers;

public class SaveLoadedHandlerTests
{
	private Mock<IModHelper> _mockHelper;
	private Mock<IMonitor> _mockMonitor;
	private Mock<IEconomyService> _mockEconomyService;
	private MockGameLoopEvents _mockGameLoopEvents;
	private SaveLoadedHandler _handler;

	[SetUp]
	public void Setup()
	{
		_mockHelper = new Mock<IModHelper>();
		_mockMonitor = new Mock<IMonitor>();
		_mockEconomyService = new Mock<IEconomyService>();
		_mockGameLoopEvents = new MockGameLoopEvents();

		var mockEvents = new Mock<IModEvents>();

		_mockHelper.Setup(m => m.Events).Returns(mockEvents.Object);
		mockEvents.Setup(m => m.GameLoop).Returns(_mockGameLoopEvents);

		_handler = new SaveLoadedHandler(_mockHelper.Object, _mockMonitor.Object, _mockEconomyService.Object);
		_handler.Register();
	}

	[Test]
	public void ShouldForwardSaveLoaded()
	{
		_mockGameLoopEvents.InvokeSaveLoaded();
		_mockEconomyService.Verify(m => m.OnLoaded(), Times.Once);
	}
}