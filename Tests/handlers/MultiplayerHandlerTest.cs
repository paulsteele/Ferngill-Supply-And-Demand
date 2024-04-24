using System.Runtime.CompilerServices;
using fse.core.handlers;
using fse.core.models;
using fse.core.multiplayer;
using fse.core.services;
using Moq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Tests.HarmonyMocks;
using Object = StardewValley.Object;

namespace Tests.handlers;

[TestFixture]
public class MultiplayerHandlerTests : HarmonyTestBase
{
	private Mock<IModHelper> _mockModHelper;
	private Mock<IEconomyService> _mockEconomyService;
	private Mock<IMultiplayerService> _mockMultiplayerService;

	private Farmer _farmer1;
	private Farmer _farmer2;
	private Farmer _farmer3;
	private Farmer _farmer4;
	
	private MultiplayerHandler _handler;
	private MockMultiplayerEvents _mockMultiplayerEvents;

	[SetUp]
	public override void Setup()
	{
		base.Setup();

		_farmer1 = new Farmer();
		_farmer2 = new Farmer();
		_farmer3 = new Farmer();
		_farmer4 = new Farmer();

		_mockModHelper = new Mock<IModHelper>();
		_mockEconomyService = new Mock<IEconomyService>();
		_mockMultiplayerService = new Mock<IMultiplayerService>();
		_mockMultiplayerEvents = new MockMultiplayerEvents();

		var mockEvents = new Mock<IModEvents>();
		mockEvents.Setup(m => m.Multiplayer).Returns(_mockMultiplayerEvents);
		_mockModHelper.Setup(m => m.Events).Returns(mockEvents.Object);
		
		_handler = new MultiplayerHandler(_mockModHelper.Object, _mockEconomyService.Object, _mockMultiplayerService.Object);
		
		HarmonyFarmer.IsMainPlayerDictionary.Add(_farmer1, true);
		HarmonyFarmer.IsMainPlayerDictionary.Add(_farmer2, false);
		HarmonyFarmer.IsMainPlayerDictionary.Add(_farmer3, false);
		HarmonyFarmer.IsMainPlayerDictionary.Add(_farmer4, false);

		HarmonyGame.GetPlayerResult = _farmer1;
		
		_handler.Register();
	}

	[Test]
	public void ShouldReceiveEconomyMessage()
	{
		HarmonyGame.GetPlayerResult = _farmer1;

		var economy1 = new EconomyModel();
		var economy2 = new EconomyModel();
		var economy3 = new EconomyModel();
		var economy4 = new EconomyModel();

		_mockMultiplayerService.Setup(m =>
				m.IsMultiplayerMessageOfType(EconomyModelMessage.StaticType, It.IsAny<ModMessageReceivedEventArgs>()))
			.Returns(true);
		
		HarmonyGame.GetPlayerResult = _farmer1;
		_mockMultiplayerEvents.InvokeModMessageReceived(new EconomyModelMessage(economy1));
		_mockEconomyService.Verify(m => m.ReceiveEconomy(economy1), Times.Never);
		
		HarmonyGame.GetPlayerResult = _farmer2;
		_mockMultiplayerEvents.InvokeModMessageReceived(new EconomyModelMessage(economy2));
		_mockEconomyService.Verify(m => m.ReceiveEconomy(economy2), Times.Once);
		
		HarmonyGame.GetPlayerResult = _farmer3;
		_mockMultiplayerEvents.InvokeModMessageReceived(new EconomyModelMessage(economy3));
		_mockEconomyService.Verify(m => m.ReceiveEconomy(economy3), Times.Once);
		
		HarmonyGame.GetPlayerResult = _farmer4;
		_mockMultiplayerEvents.InvokeModMessageReceived(new EconomyModelMessage(economy4));
		_mockEconomyService.Verify(m => m.ReceiveEconomy(economy4), Times.Once);
	}
	
	[Test]
	public void ShouldReceiveEconomyRequestedMessage()
	{
		HarmonyGame.GetPlayerResult = _farmer1;

		_mockMultiplayerService.Setup(m =>
				m.IsMultiplayerMessageOfType(RequestEconomyModelMessage.StaticType, It.IsAny<ModMessageReceivedEventArgs>()))
			.Returns(true);
		
		HarmonyGame.GetPlayerResult = _farmer2;
		_mockMultiplayerEvents.InvokeModMessageReceived(new RequestEconomyModelMessage());
		_mockEconomyService.Verify(m => m.SendEconomyMessage(), Times.Never);
		
		HarmonyGame.GetPlayerResult = _farmer3;
		_mockMultiplayerEvents.InvokeModMessageReceived(new RequestEconomyModelMessage());
		_mockEconomyService.Verify(m => m.SendEconomyMessage(), Times.Never);
		
		HarmonyGame.GetPlayerResult = _farmer4;
		_mockMultiplayerEvents.InvokeModMessageReceived(new RequestEconomyModelMessage());
		_mockEconomyService.Verify(m => m.SendEconomyMessage(), Times.Never);
		
		HarmonyGame.GetPlayerResult = _farmer1;
		_mockMultiplayerEvents.InvokeModMessageReceived(new RequestEconomyModelMessage());
		_mockEconomyService.Verify(m => m.SendEconomyMessage(), Times.Once);
	}
	
	[Test]
	public void ShouldReceiveSupplyAdjustedMessage()
	{
		HarmonyGame.GetPlayerResult = _farmer1;

		_mockMultiplayerService.Setup(m =>
				m.IsMultiplayerMessageOfType(SupplyAdjustedMessage.StaticType, It.IsAny<ModMessageReceivedEventArgs>()))
			.Returns(true);
		
		HarmonyGame.GetPlayerResult = _farmer1;
		_mockMultiplayerEvents.InvokeModMessageReceived(new SupplyAdjustedMessage("(O)72", 1));
		_mockEconomyService.Verify(m => m.AdjustSupply(
			It.Is<Object>(o => o.ItemId == "(O)72"), 1, false), Times.Once);
		
		HarmonyGame.GetPlayerResult = _farmer2;
		_mockMultiplayerEvents.InvokeModMessageReceived(new SupplyAdjustedMessage("(O)73", 2));
		_mockEconomyService.Verify(m => m.AdjustSupply(
			It.Is<Object>(o => o.ItemId == "(O)73"), 2, false), Times.Once);
		
		HarmonyGame.GetPlayerResult = _farmer3;
		_mockMultiplayerEvents.InvokeModMessageReceived(new SupplyAdjustedMessage("(O)74", 3));
		_mockEconomyService.Verify(m => m.AdjustSupply(
			It.Is<Object>(o => o.ItemId == "(O)74"), 3, false), Times.Once);
		
		HarmonyGame.GetPlayerResult = _farmer4;
		_mockMultiplayerEvents.InvokeModMessageReceived(new SupplyAdjustedMessage("(O)75", 4));
		_mockEconomyService.Verify(m => m.AdjustSupply(
			It.Is<Object>(o => o.ItemId == "(O)75"), 4, false), Times.Once);
	}
}

internal class MockMultiplayerEvents : IMultiplayerEvents
{
	public event EventHandler<PeerContextReceivedEventArgs>? PeerContextReceived;
	public event EventHandler<PeerConnectedEventArgs>? PeerConnected;
	public event EventHandler<ModMessageReceivedEventArgs>? ModMessageReceived;
	public event EventHandler<PeerDisconnectedEventArgs>? PeerDisconnected;

	public void InvokeModMessageReceived(IMessage message)
	{
		var args = (ModMessageReceivedEventArgs) RuntimeHelpers.GetUninitializedObject(typeof(ModMessageReceivedEventArgs));

		HarmonyModMessageReceivedEventArgs.ReadAsMessage = message;
		
		ModMessageReceived?.Invoke(this, args);
	}
}