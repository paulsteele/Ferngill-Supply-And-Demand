using fse.core.handlers;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Moq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Inventories;
using Tests.HarmonyMocks;
using Object = StardewValley.Object;

namespace Tests.handlers;

[TestFixture]
public class DayEndHandlerTests : HarmonyTestBase
{
	private Mock<IModHelper> _mockModHelper;
	private MockGameLoopEvents _mockGameLoopEvents;
	private Mock<IMonitor> _mockMonitor;
	private Mock<IEconomyService> _mockEconomyService;

	private Farmer _farmer1;
	private Farmer _farmer2;
	private Farmer _farmer3;
	private Farmer _farmer4;
	private MockInventory _farmer1BinInventory;
	private MockInventory _farmer2BinInventory;
	private MockInventory _farmer3BinInventory;
	private MockInventory _farmer4BinInventory;

	private Farmer[] _farmers;
	private FarmerTeam _farmerTeam;
	private Farm _farm;
	
	private DayEndHandler _handler;

	[SetUp]
	public override void Setup()
	{
		base.Setup();
		_mockModHelper = new Mock<IModHelper>();
		_mockMonitor = new Mock<IMonitor>();
		_mockEconomyService = new Mock<IEconomyService>();
		_mockGameLoopEvents = new MockGameLoopEvents();

		var mockEvents = new Mock<IModEvents>();
		_mockModHelper.Setup(m => m.Events).Returns(mockEvents.Object);
		mockEvents.Setup(m => m.GameLoop).Returns(_mockGameLoopEvents);
		
		_farm = new Farm();
		_farmerTeam = new FarmerTeam();
		_farmer1 = new Farmer();
		_farmer2 = new Farmer();
		_farmer3 = new Farmer();
		_farmer4 = new Farmer();
		_farmer1BinInventory = [
			new Object("(O)72", 4),
			new Object("(O)73", 2),
			new Object("(O)74", 1),
		];
		_farmer2BinInventory = [
			new Object("(O)75", 7),
			new Object("(O)76", 9),
		];
		_farmer3BinInventory = [
			new Object("(O)77", 8),
			new Object("(O)78", 3),
		];
		_farmer4BinInventory = [
			new Object("(O)79", 2),
		];
		
		_farmers =
		[
			_farmer1,
			_farmer2,
			_farmer3,
			_farmer4,
		];

		_farmerTeam.SetUseSeparateWalletsResult(true);

		HarmonyFarmer.FarmerTeamDictionary.Add(_farmer1, _farmerTeam);
		HarmonyFarmer.FarmerTeamDictionary.Add(_farmer2, _farmerTeam);
		HarmonyFarmer.FarmerTeamDictionary.Add(_farmer3, _farmerTeam);
		HarmonyFarmer.FarmerTeamDictionary.Add(_farmer4, _farmerTeam);
		HarmonyFarmer.IsMainPlayerDictionary.Add(_farmer1, true);
		HarmonyFarmer.IsMainPlayerDictionary.Add(_farmer2, false);
		HarmonyFarmer.IsMainPlayerDictionary.Add(_farmer3, false);
		HarmonyFarmer.IsMainPlayerDictionary.Add(_farmer4, false);
		HarmonyGame.GetAllFarmersResults = _farmers;
		HarmonyGame.GetPlayerResult = _farmer1;
		HarmonyGame.GetFarmResult = _farm;
		HarmonyFarm.GetShippingBinDictionary.Add(_farmer1, _farmer1BinInventory);
		HarmonyFarm.GetShippingBinDictionary.Add(_farmer2, _farmer2BinInventory);
		HarmonyFarm.GetShippingBinDictionary.Add(_farmer3, _farmer3BinInventory);
		HarmonyFarm.GetShippingBinDictionary.Add(_farmer4, _farmer4BinInventory);
		
		_handler = new DayEndHandler(_mockModHelper.Object, _mockMonitor.Object, _mockEconomyService.Object);
		_handler.Register();
	}

	[Test]
	public void ShouldAdvanceOneDayOnDayStarting()
	{
		_mockEconomyService.Verify(m => m.AdvanceOneDay(), Times.Never);
		
		_mockGameLoopEvents.InvokeDayStarted();
		
		_mockEconomyService.Verify(m => m.AdvanceOneDay(), Times.Once);
	}

	[Test]
	public void ShouldAdjustSupplyForSinglePlayer()
	{
		_farmers = [_farmer1];
		
		HarmonyGame.GetAllFarmersResults = _farmers;
		
		_mockGameLoopEvents.InvokeDayEnding();
		
		_mockEconomyService.Verify(m => m.AdjustSupply(It.IsAny<Object>(), It.IsAny<int>(), false), Times.Exactly(_farmer1BinInventory.Count));
		foreach (var item in _farmer1BinInventory)
		{
			_mockEconomyService.Verify(m => m.AdjustSupply(item as Object, item.Stack, false));
		}
	}
	
	[Test]
	public void ShouldAdjustSupplyForMultiplePlayersWhoShareATeam()
	{
		_farmerTeam.SetUseSeparateWalletsResult(false);
		
		_mockGameLoopEvents.InvokeDayEnding();
		
		_mockEconomyService.Verify(m => m.AdjustSupply(It.IsAny<Object>(), It.IsAny<int>(), false), Times.Exactly(_farmer1BinInventory.Count));
		foreach (var item in _farmer1BinInventory)
		{
			_mockEconomyService.Verify(m => m.AdjustSupply(item as Object, item.Stack, false));
		}
	}
	
	[Test]
	public void ShouldAdjustSupplyForMultiplePlayersWhoDoNotShareATeam()
	{
		_farmerTeam.SetUseSeparateWalletsResult(true);
		
		_mockGameLoopEvents.InvokeDayEnding();
		
		_mockEconomyService.Verify(m => m.AdjustSupply(It.IsAny<Object>(), It.IsAny<int>(), false), Times.Exactly(_farmer1BinInventory.Count + _farmer2BinInventory.Count + _farmer3BinInventory.Count + _farmer4BinInventory.Count));
		foreach (var item in _farmer1BinInventory.Concat(_farmer2BinInventory).Concat(_farmer3BinInventory).Concat(_farmer4BinInventory))
		{
			_mockEconomyService.Verify(m => m.AdjustSupply(item as Object, item.Stack, false));
		}
	}
	
	[Test]
	public void ShouldNotDoAnythingIfNotTheMainPlayer()
	{
		HarmonyGame.GetPlayerResult = _farmer2;
		_farmerTeam.SetUseSeparateWalletsResult(true);
		
		_mockGameLoopEvents.InvokeDayEnding();

		_mockEconomyService.Verify(m => m.AdjustSupply(It.IsAny<Object>(), It.IsAny<int>(), false), Times.Never);
	}
	
	[TestCase(Season.Spring, true, false)]
	[TestCase(Season.Summer, true, false)]
	[TestCase(Season.Fall, true, false)]
	[TestCase(Season.Winter, false, true)]
	public void ShouldAdvanceToCorrectStateAtEndOfSeason(Season season, bool shouldAdvanceSeason, bool shouldAdvanceYear)
	{
		_farmerTeam.SetUseSeparateWalletsResult(false);
		Game1.dayOfMonth = 28;
		Game1.season = season;
		
		_mockGameLoopEvents.InvokeDayEnding();
		
		_mockEconomyService.Verify(m => m.SetupForNewSeason(), Times.Exactly(shouldAdvanceSeason ? 1 : 0));
		_mockEconomyService.Verify(m => m.SetupForNewYear(), Times.Exactly(shouldAdvanceYear ? 1 : 0));
	}
}

internal class MockInventory : List<Item>, IInventory
{
	public bool HasAny() => throw new NotImplementedException();

	public bool HasEmptySlots() => throw new NotImplementedException();

	public int CountItemStacks() => throw new NotImplementedException();

	public void OverwriteWith(IList<Item> list)
	{
		throw new NotImplementedException();
	}

	public IList<Item> GetRange(int index, int count) => throw new NotImplementedException();

	public void AddRange(ICollection<Item> collection)
	{
		throw new NotImplementedException();
	}

	public void RemoveEmptySlots()
	{
		throw new NotImplementedException();
	}

	public bool ContainsId(string itemId) => throw new NotImplementedException();

	public bool ContainsId(string itemId, int minimum) => throw new NotImplementedException();

	public int CountId(string itemId) => throw new NotImplementedException();

	public IEnumerable<Item> GetById(string itemId) => throw new NotImplementedException();

	public int ReduceId(string itemId, int count) => throw new NotImplementedException();

	public bool RemoveButKeepEmptySlot(Item item) => throw new NotImplementedException();

	public long LastTickSlotChanged { get; }
}

internal class MockGameLoopEvents : IGameLoopEvents
{
	public event EventHandler<GameLaunchedEventArgs>? GameLaunched;
	public event EventHandler<UpdateTickingEventArgs>? UpdateTicking;
	public event EventHandler<UpdateTickedEventArgs>? UpdateTicked;
	public event EventHandler<OneSecondUpdateTickingEventArgs>? OneSecondUpdateTicking;
	public event EventHandler<OneSecondUpdateTickedEventArgs>? OneSecondUpdateTicked;
	public event EventHandler<SaveCreatingEventArgs>? SaveCreating;
	public event EventHandler<SaveCreatedEventArgs>? SaveCreated;
	public event EventHandler<SavingEventArgs>? Saving;
	public event EventHandler<SavedEventArgs>? Saved;
	public event EventHandler<SaveLoadedEventArgs>? SaveLoaded;
	public event EventHandler<DayStartedEventArgs>? DayStarted;
	public event EventHandler<DayEndingEventArgs>? DayEnding;
	public event EventHandler<TimeChangedEventArgs>? TimeChanged;
	public event EventHandler<ReturnedToTitleEventArgs>? ReturnedToTitle;

	public void InvokeDayStarted()
	{
		DayStarted?.Invoke(this, new DayStartedEventArgs());
	}
	
	public void InvokeDayEnding()
	{
		DayEnding?.Invoke(this, new DayEndingEventArgs());
	}
}