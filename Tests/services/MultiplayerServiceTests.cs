using fse.core.multiplayer;
using fse.core.services;
using Moq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;
using Tests.HarmonyMocks;
using Range = System.Range;

namespace Tests.services;

public class MultiplayerServiceTests : HarmonyTestBase
{
	private Mock<IModHelper> _mockModHelper;
	private Mock<IMultiplayerHelper> _mockMultiplayerHelper;
	private Mock<IModContentHelper> _mockModContentHelper;
	private MultiplayerService _multiplayerService;

	private Farmer _farmer1;
	private Farmer _farmer2;
	private Farmer _farmer3;
	private Farmer _farmer4;

	private Farmer[] _farmers;

	[SetUp]
	public override void Setup()
	{
		base.Setup();
		_mockModHelper = new Mock<IModHelper>();
		_mockMultiplayerHelper = new Mock<IMultiplayerHelper>(MockBehavior.Loose);
		_mockModContentHelper = new Mock<IModContentHelper>();

		_mockModHelper.SetupGet(m => m.Multiplayer).Returns(_mockMultiplayerHelper.Object);
		_mockModHelper.SetupGet(m => m.ModContent).Returns(_mockModContentHelper.Object);

		_mockModContentHelper.SetupGet(m => m.ModID).Returns("mod-id");

		_farmer1 = new Farmer();
		_farmer2 = new Farmer();
		_farmer3 = new Farmer();
		_farmer4 = new Farmer();
		
		HarmonyFarmer.UniqueMultiplayerIdDictionary.Add(_farmer1, 1);
		HarmonyFarmer.UniqueMultiplayerIdDictionary.Add(_farmer2, 2);
		HarmonyFarmer.UniqueMultiplayerIdDictionary.Add(_farmer3, 3);
		HarmonyFarmer.UniqueMultiplayerIdDictionary.Add(_farmer4, 4);
		
		_farmers =
		[
			_farmer1,
			_farmer2,
			_farmer3,
			_farmer4,
		];

		var farmerCollection = new FarmerCollection();

		HarmonyGame.GetPlayerResult = _farmer1;
		HarmonyGame.GetOnlineFarmersResults = farmerCollection;
		HarmonyFarmerCollection.CollectionEnumerator = _farmers.GetEnumerator();

		_multiplayerService = new MultiplayerService(_mockModHelper.Object);
	}

	[Test]
	public void ShouldSendMessageToOtherPlayers(
		[Values(0, 1, 2, 3)] int currentFarmerIndex,
		[Values(1, 2, 3, 4)] int numberOfFarmers
	)
	{
		if (currentFarmerIndex >= numberOfFarmers)
		{
			Assert.Pass("N/A");
			return;
		}

		_farmers = _farmers.Take(numberOfFarmers).ToArray();
		HarmonyFarmerCollection.CollectionEnumerator = _farmers.GetEnumerator();
		HarmonyGame.GetPlayerResult = _farmers[currentFarmerIndex];

		var expectedLongList = new []{1L, 2L, 3L, 4L}.Take(numberOfFarmers).ToList();
		expectedLongList.Remove(currentFarmerIndex + 1);
		
		var message = new TestMessage();
		_multiplayerService.SendMessageToPeers(message);
		
		_mockMultiplayerHelper.Verify(m => m.SendMessage(
			It.Is<IMessage>(m => m == message),
			message.Type,
			It.Is<string[]>(strings => string.Equals(strings[0], "mod-id")),
			expectedLongList.ToArray()
		), 
		Times.Exactly(numberOfFarmers == 1 ? 0 : 1));
	}

	class TestMessage : IMessage
	{
		public string Type => nameof(TestMessage);
	}
}