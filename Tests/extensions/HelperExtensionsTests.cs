using fse.core.extensions;
using fse.core.multiplayer;
using Moq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using Tests.HarmonyMocks;

namespace Tests.extensions;

public class HelperExtensionsTests : HarmonyTestBase
{
	private Mock<IModHelper> _mockModHelper;
	private Mock<IMultiplayerHelper> _mockMultiplayerHelper;
	private Mock<IModContentHelper> _mockModContentHelper;

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
		_mockMultiplayerHelper = new Mock<IMultiplayerHelper>();
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
	}

	[Test]
	public void ShouldSendMessageToOtherPlayers()
	{
		var message = new TestMessage();
		_mockModHelper.Object.SendMessageToPeers(message);

		_mockMultiplayerHelper.Verify(m => m.SendMessage(
			It.Is<IMessage>(m => m == message),
			message.Type,
			It.Is<string[]>(strings => string.Equals(strings[0], "mod-id")),
			It.Is<long[]>(longs => longs[0] == 2L && longs[1] == 3L && longs[2] == 4L)
		), 
		Times.Once);
	}
	
	class TestMessage : IMessage
	{
		public string Type => nameof(TestMessage);
	}
}