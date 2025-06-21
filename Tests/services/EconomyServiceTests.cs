using fse.core.models;
using fse.core.multiplayer;
using fse.core.services;
using Moq;
using NUnit.Framework.Constraints;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using Tests.HarmonyMocks;
using Object = StardewValley.Object;

namespace Tests.services;

public class EconomyServiceTests : HarmonyTestBase
{
	private Mock<IModHelper> _mockModHelper;
	private Mock<IDataHelper> _mockDataHelper;
	private Mock<IMonitor> _mockMonitor;
	private Mock<IMultiplayerService> _mockMultiplayerService;
	private Mock<IFishService> _mockFishService;
	private Mock<ISeedService> _mockSeedService;
	private Mock<IArtisanService> _mockArtisanService;
	private Mock<INormalDistributionService> _mockNormalDistributionService;
	private Mock<IUpdateFrequencyService> _mockUpdateFrequencyService;
	private Farmer _player;

	private EconomyService _economyService;

	[SetUp]
	public override void Setup()
	{
		base.Setup();

		ConfigModel.Instance = new ConfigModel()
		{
			ValidCategories = [1, 2, 3, 4, 5],
		};

		_mockModHelper = new Mock<IModHelper>();
		_mockDataHelper = new Mock<IDataHelper>();
		_mockMonitor = new Mock<IMonitor>();
		_mockMultiplayerService = new Mock<IMultiplayerService>();
		_mockFishService = new Mock<IFishService>();
		_mockSeedService = new Mock<ISeedService>();
		_mockArtisanService = new Mock<IArtisanService>();
		_mockNormalDistributionService = new Mock<INormalDistributionService>();
		_mockUpdateFrequencyService = new Mock<IUpdateFrequencyService>();
		_player = new Farmer();

		_mockModHelper.Setup(m => m.Data).Returns(_mockDataHelper.Object);
		_mockDataHelper.Setup(m => m.ReadSaveData<EconomyModel>(EconomyModel.ModelKey))
			.Returns((EconomyModel)null!);
		
		_mockNormalDistributionService.Setup(m => m.SampleSupply()).Returns(777);
		_mockNormalDistributionService.Setup(m => m.SampleSeasonlessDelta()).Returns(27);
		_mockNormalDistributionService.Setup(m => m.SampleInSeasonDelta()).Returns(26);
		_mockNormalDistributionService.Setup(m => m.SampleOutOfSeasonDelta()).Returns(25);

		_mockArtisanService.Setup(m => m.GetBaseFromArtisanGood("307")).Returns(new ItemModel("442"));

		Game1.objectData = new Dictionary<string, ObjectData>(new[]
		{
			GenerateObjectData("1", 1),
			GenerateObjectData("2", 1),
			GenerateObjectData("3", 2),
			GenerateObjectData("4", 2),
		});
		
		HarmonyObject.CategoryIdToNameMapping.Add(1, "Cat1");
		HarmonyObject.CategoryIdToNameMapping.Add(2, "Cat2");

		HarmonyGame.GetPlayerResult = _player; 
		HarmonyFarmer.IsMainPlayerDictionary.Add(_player, true);

		_economyService = new EconomyService
		(
			_mockModHelper.Object,
			_mockMonitor.Object,
			_mockMultiplayerService.Object,
			_mockFishService.Object,
			_mockSeedService.Object,
			_mockArtisanService.Object,
			_mockNormalDistributionService.Object,
			_mockUpdateFrequencyService.Object
		);
	}

	[Test]
	public void ShouldGenerateNewEconomyOnLoadIfEmpty()
	{
		_economyService.OnLoaded();
		
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.IsAny<RequestEconomyModelMessage>()), Times.Exactly(0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(1));
		_mockFishService.Verify(m => m.GenerateFishMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		_mockSeedService.Verify(m => m.GenerateSeedMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		Assert.That(_economyService.Loaded, Is.True);

		var categories = _economyService.GetCategories();
		
		Assert.That(categories, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{ 
			Assert.That(categories[1], Is.EqualTo("Cat1")); 
			Assert.That(categories[2], Is.EqualTo("Cat2"));
		});

		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);
		
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items, Has.Length.EqualTo(2)); 
			Assert.That(cat2Items, Has.Length.EqualTo(2));
		});
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items[0].ObjectId, Is.EqualTo("1")); 
			Assert.That(cat1Items[1].ObjectId, Is.EqualTo("2")); 
			Assert.That(cat2Items[0].ObjectId, Is.EqualTo("3")); 
			Assert.That(cat2Items[1].ObjectId, Is.EqualTo("4"));
			
			Assert.That(cat1Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat1Items[1].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[1].Supply, Is.EqualTo(777));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
	}
	
	[Test]
	public void ShouldGenerateNewEconomyOnLoadIfEmptyWithCurrentSeasonsCrop()
	{
		Game1.currentSeason = "Summer";
		var seed1 = new SeedModel("1", new CropData())
		{
			Seasons = Seasons.Summer,
		};
		var seed2 = new SeedModel("2", new CropData())
		{
			Seasons = Seasons.Fall,
		};
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("1")).Returns(seed1);
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("2")).Returns(seed2);
		
		_economyService.OnLoaded();
		
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.IsAny<RequestEconomyModelMessage>()), Times.Exactly(0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(1));
		_mockFishService.Verify(m => m.GenerateFishMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		_mockSeedService.Verify(m => m.GenerateSeedMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		Assert.That(_economyService.Loaded, Is.True);

		var categories = _economyService.GetCategories();
		
		Assert.That(categories, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{ 
			Assert.That(categories[1], Is.EqualTo("Cat1")); 
			Assert.That(categories[2], Is.EqualTo("Cat2"));
		});

		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);
		
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items, Has.Length.EqualTo(2)); 
			Assert.That(cat2Items, Has.Length.EqualTo(2));
		});
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items[0].ObjectId, Is.EqualTo("1")); 
			Assert.That(cat1Items[1].ObjectId, Is.EqualTo("2")); 
			Assert.That(cat2Items[0].ObjectId, Is.EqualTo("3")); 
			Assert.That(cat2Items[1].ObjectId, Is.EqualTo("4"));
			
			Assert.That(cat1Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat1Items[1].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[1].Supply, Is.EqualTo(777));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(26)); 
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(25)); 
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
	}
	
	[Test]
	public void ShouldGenerateNewEconomyOnLoadIfEmptyWithCurrentSeasonsFish()
	{
		Game1.currentSeason = "Summer";
		var fish1 = new FishModel("1","a/b/c/d/e/f/Spring")
		{
			Seasons = Seasons.Summer,
		};
		var fish2 = new FishModel("2","a/b/c/d/e/f/Spring")
		{
			Seasons = Seasons.Fall,
		};
		_mockFishService.Setup(m => m.GetFishModelFromModelId("1")).Returns(fish1);
		_mockFishService.Setup(m => m.GetFishModelFromModelId("2")).Returns(fish2);
		
		_economyService.OnLoaded();
		
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.IsAny<RequestEconomyModelMessage>()), Times.Exactly(0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(1));
		_mockFishService.Verify(m => m.GenerateFishMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		_mockSeedService.Verify(m => m.GenerateSeedMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		Assert.That(_economyService.Loaded, Is.True);

		var categories = _economyService.GetCategories();
		
		Assert.That(categories, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{ 
			Assert.That(categories[1], Is.EqualTo("Cat1")); 
			Assert.That(categories[2], Is.EqualTo("Cat2"));
		});

		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);
		
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items, Has.Length.EqualTo(2)); 
			Assert.That(cat2Items, Has.Length.EqualTo(2));
		});
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items[0].ObjectId, Is.EqualTo("1")); 
			Assert.That(cat1Items[1].ObjectId, Is.EqualTo("2")); 
			Assert.That(cat2Items[0].ObjectId, Is.EqualTo("3")); 
			Assert.That(cat2Items[1].ObjectId, Is.EqualTo("4"));
			
			Assert.That(cat1Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat1Items[1].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[1].Supply, Is.EqualTo(777));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(26)); 
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(25)); 
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
	}

	[Test]
	public void ShouldConsolidateCategoriesWithTheSameName()
	{
		HarmonyObject.CategoryIdToNameMapping.Clear();
		HarmonyObject.CategoryIdToNameMapping.Add(1, "Cat1");
		HarmonyObject.CategoryIdToNameMapping.Add(2, "Cat1");
		
		_economyService.OnLoaded();
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);
		
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items, Has.Length.EqualTo(4)); 
			Assert.That(cat2Items, Has.Length.EqualTo(2));
		});
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items[0].ObjectId, Is.EqualTo("1")); 
			Assert.That(cat1Items[1].ObjectId, Is.EqualTo("2")); 
			Assert.That(cat1Items[2].ObjectId, Is.EqualTo("3")); 
			Assert.That(cat1Items[3].ObjectId, Is.EqualTo("4"));
			Assert.That(cat2Items[0].ObjectId, Is.EqualTo("3")); 
			Assert.That(cat2Items[1].ObjectId, Is.EqualTo("4"));
		});
	}
	
	[Test]
	public void ShouldLoadAnExistingEconomyOnLoad()
	{
		var model = new EconomyModel(new Dictionary<int, Dictionary<string, ItemModel>>
			{
				{1, new Dictionary<string, ItemModel>()
				{
					{"1", new ItemModel("1"){ DailyDelta = 11, Supply = 110}},
					{"2", new ItemModel("2"){ DailyDelta = 12, Supply = 120}},
				}},
				{2, new Dictionary<string, ItemModel>()
				{
					{"3", new ItemModel("3"){ DailyDelta = 21, Supply = 210}},
					{"4", new ItemModel("4"){ DailyDelta = 22, Supply = 220}},
				}},
			}
		);
		
		_mockDataHelper.Setup(m => m.ReadSaveData<EconomyModel>(EconomyModel.ModelKey))
			.Returns(model);

		_economyService.OnLoaded();
		
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.IsAny<RequestEconomyModelMessage>()), Times.Exactly(0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(0));
		_mockFishService.Verify(m => m.GenerateFishMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		_mockSeedService.Verify(m => m.GenerateSeedMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		Assert.That(_economyService.Loaded, Is.True);

		var categories = _economyService.GetCategories();
		
		Assert.That(categories, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{ 
			Assert.That(categories[1], Is.EqualTo("Cat1")); 
			Assert.That(categories[2], Is.EqualTo("Cat2"));
		});

		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);
		
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items, Has.Length.EqualTo(2)); 
			Assert.That(cat2Items, Has.Length.EqualTo(2));
		});
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items[0].ObjectId, Is.EqualTo("1")); 
			Assert.That(cat1Items[1].ObjectId, Is.EqualTo("2")); 
			Assert.That(cat2Items[0].ObjectId, Is.EqualTo("3")); 
			Assert.That(cat2Items[1].ObjectId, Is.EqualTo("4"));
			
			Assert.That(cat1Items[0].Supply, Is.EqualTo(110)); 
			Assert.That(cat1Items[1].Supply, Is.EqualTo(120)); 
			Assert.That(cat2Items[0].Supply, Is.EqualTo(210)); 
			Assert.That(cat2Items[1].Supply, Is.EqualTo(220));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(11)); 
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(12)); 
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(21)); 
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(22));
		});
	}

	[Test]
	public void ShouldRequestEconomyIfClient()
	{
		HarmonyFarmer.IsMainPlayerDictionary.Clear();
		HarmonyFarmer.IsMainPlayerDictionary.Add(_player, false);
		
		_economyService.OnLoaded();
		
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.IsAny<RequestEconomyModelMessage>()), Times.Exactly(1));
		_mockDataHelper.Verify(m => m.ReadSaveData<EconomyModel>(EconomyModel.ModelKey), Times.Exactly(0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(0));
		_mockFishService.Verify(m => m.GenerateFishMapping(It.IsAny<EconomyModel>()), Times.Exactly(0));
		_mockSeedService.Verify(m => m.GenerateSeedMapping(It.IsAny<EconomyModel>()), Times.Exactly(0));
		Assert.That(_economyService.Loaded, Is.False);
	}

	[Test]
	public void ShouldHandleEndOfDay([Values] bool shouldUpdateSupply, [Values] bool shouldUpdateDelta, [Values]Seasons updateSeason)
	{
		_economyService.OnLoaded();
		_mockNormalDistributionService.Invocations.Clear();
		_mockDataHelper.Invocations.Clear();
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);
		
		var seed1 = new SeedModel("1", new CropData())
		{
			Seasons = updateSeason,
		};
		var outOfSeason = updateSeason == Seasons.Spring ? Seasons.Winter : Seasons.Spring;
		var seed2 = new SeedModel("2", new CropData())
		{
			Seasons = outOfSeason,
		};
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("1")).Returns(seed1);
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("2")).Returns(seed2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;

		var day = new DayModel(6, Seasons.Winter, 28);
		_mockUpdateFrequencyService.Setup(m => m.GetUpdateFrequencyInformation(day))
			.Returns(new UpdateFrequencyModel(shouldUpdateSupply, shouldUpdateDelta, updateSeason));
		_economyService.HandleDayEnd(day);

		var expectedSupply = shouldUpdateSupply ? 777 : 12;
		var expectedSeasonlessDelta = shouldUpdateDelta ? 27 : 12;
		var expectedOutOfSeasonDelta = shouldUpdateDelta ? 26 : 12;
		var expectedInSeasonDelta = shouldUpdateDelta ? 25 : 12;

		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(expectedSupply)); 
			Assert.That(cat1Items[1].Supply, Is.EqualTo(expectedSupply)); 
			Assert.That(cat2Items[0].Supply, Is.EqualTo(expectedSupply)); 
			Assert.That(cat2Items[1].Supply, Is.EqualTo(expectedSupply));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(expectedOutOfSeasonDelta)); 
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(expectedInSeasonDelta)); 
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(expectedSeasonlessDelta)); 
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(expectedSeasonlessDelta));
		});

		_mockNormalDistributionService.Verify(m => m.Reset(), Times.Exactly(shouldUpdateSupply || shouldUpdateDelta ? 1 : 0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly( shouldUpdateSupply || shouldUpdateDelta ? 1 : 0));
	}

	[Test]
	public void ShouldSetupForNewSeason()
	{
		_economyService.OnLoaded();
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;

		var day = new DayModel(2, Seasons.Summer, 2);
		_mockUpdateFrequencyService.Setup(m => m.GetUpdateFrequencyInformation(day))
			.Returns(new UpdateFrequencyModel(false, true, Seasons.Summer));
		_economyService.HandleDayEnd(day);

		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat1Items[1].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[1].Supply, Is.EqualTo(12));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(27));
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(27));
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27));
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
		
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(2));
	}
	
	[Test]
	public void ShouldSetupForNewSeasonWithNextSeasonsCrops()
	{
		_economyService.OnLoaded();

		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;

		var seed1 = new SeedModel("1", new CropData())
		{
			Seasons = Seasons.Summer,
		};
		var seed2 = new SeedModel("2", new CropData())
		{
			Seasons = Seasons.Fall,
		};
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("1")).Returns(seed1);
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("2")).Returns(seed2);
		
		var day = new DayModel(2, Seasons.Summer, 28);
		_mockUpdateFrequencyService.Setup(m => m.GetUpdateFrequencyInformation(day))
			.Returns(new UpdateFrequencyModel(false, true, Seasons.Fall));
		_economyService.HandleDayEnd(day);

		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat1Items[1].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[1].Supply, Is.EqualTo(12));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(25));
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(26));
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27));
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
		
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(2));
	}
	
	[Test]
	public void ShouldSetupForNewSeasonWithNextSeasonsFish()
	{
		_economyService.OnLoaded();

		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;

		var fish1 = new FishModel("1","a/b/c/d/e/f/Spring")
		{
			Seasons = Seasons.Summer,
		};
		var fish2 = new FishModel("2","a/b/c/d/e/f/Spring")
		{
			Seasons = Seasons.Fall,
		};
		_mockFishService.Setup(m => m.GetFishModelFromModelId("1")).Returns(fish1);
		_mockFishService.Setup(m => m.GetFishModelFromModelId("2")).Returns(fish2);
		
		var day = new DayModel(4, Seasons.Summer, 28);
		_mockUpdateFrequencyService.Setup(m => m.GetUpdateFrequencyInformation(day))
			.Returns(new UpdateFrequencyModel(false, true, Seasons.Fall));
		_economyService.HandleDayEnd(day);

		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat1Items[1].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[1].Supply, Is.EqualTo(12));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(25));
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(26));
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27));
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
		
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(2));
	}
	
	[Test]
	public void ShouldNotSetupForNewSeasonIfClient()
	{
		_economyService.OnLoaded();
		
		HarmonyFarmer.IsMainPlayerDictionary.Clear();
		HarmonyFarmer.IsMainPlayerDictionary.Add(_player, false);
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;
		
		var day = new DayModel(6, Seasons.Winter, 28);
		_mockUpdateFrequencyService.Setup(m => m.GetUpdateFrequencyInformation(day))
			.Returns(new UpdateFrequencyModel(false, true, Seasons.Spring));
		_economyService.HandleDayEnd(day);
		
		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(12));
			Assert.That(cat1Items[1].Supply, Is.EqualTo(12));
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(12));
			Assert.That(cat2Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(12));
			Assert.That(cat2Items[1].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(12));
		});

		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(1));
	}

	[Test]
	public void ShouldSetupForNewYear()
	{
		_economyService.OnLoaded();
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;

		var day = new DayModel(6, Seasons.Winter, 28);
		_mockUpdateFrequencyService.Setup(m => m.GetUpdateFrequencyInformation(day))
			.Returns(new UpdateFrequencyModel(true, true, Seasons.Spring));
		_economyService.HandleDayEnd(day);

		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat1Items[1].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[1].Supply, Is.EqualTo(777));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
		
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(2));
	}
	
	[Test]
	public void ShouldSetupForNewYearWithNextSeasonsCrops()
	{
		_economyService.OnLoaded();
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;
		
		var seed1 = new SeedModel("1", new CropData())
		{
			Seasons = Seasons.Winter,
		};
		var seed2 = new SeedModel("2", new CropData())
		{
			Seasons = Seasons.Spring,
		};
		
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("1")).Returns(seed1);
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("2")).Returns(seed2);
		
		var day = new DayModel(6, Seasons.Winter, 28);
		_mockUpdateFrequencyService.Setup(m => m.GetUpdateFrequencyInformation(day))
			.Returns(new UpdateFrequencyModel(true, true, Seasons.Spring));
		_economyService.HandleDayEnd(day);
		
		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat1Items[1].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[1].Supply, Is.EqualTo(777));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(25)); 
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(26)); 
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
		
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(2));
	}
	
	[Test]
	public void ShouldSetupForNewYearWithNextSeasonsFish()
	{
		_economyService.OnLoaded();
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;
		
		var fish1 = new FishModel("1","a/b/c/d/e/f/Spring")
		{
			Seasons = Seasons.Winter,
		};
		var fish2 = new FishModel("2","a/b/c/d/e/f/Spring")
		{
			Seasons = Seasons.Spring,
		};
		_mockFishService.Setup(m => m.GetFishModelFromModelId("1")).Returns(fish1);
		_mockFishService.Setup(m => m.GetFishModelFromModelId("2")).Returns(fish2);
		
		var day = new DayModel(6, Seasons.Winter, 28);
		_mockUpdateFrequencyService.Setup(m => m.GetUpdateFrequencyInformation(day))
			.Returns(new UpdateFrequencyModel(true, true, Seasons.Spring));
		_economyService.HandleDayEnd(day);
		
		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat1Items[1].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[0].Supply, Is.EqualTo(777)); 
			Assert.That(cat2Items[1].Supply, Is.EqualTo(777));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(25)); 
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(26)); 
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27)); 
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
		
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(2));
	}

	[Test]
	public void ShouldNotSetupForNewYearIfClient()
	{
		_economyService.OnLoaded();
		
		HarmonyFarmer.IsMainPlayerDictionary.Clear();
		HarmonyFarmer.IsMainPlayerDictionary.Add(_player, false);
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;
		
		var day = new DayModel(6, Seasons.Winter, 28);
		_mockUpdateFrequencyService.Setup(m => m.GetUpdateFrequencyInformation(day))
			.Returns(new UpdateFrequencyModel(true, true, Seasons.Spring));
		_economyService.HandleDayEnd(day);
		
		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(12));
			Assert.That(cat1Items[1].Supply, Is.EqualTo(12));
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(12));
			Assert.That(cat2Items[0].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(12));
			Assert.That(cat2Items[1].Supply, Is.EqualTo(12));
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(12));
		});

		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(1));
	}
	
	[Test]
	public void ShouldResetWithCurrentSeasonsCrops()
	{
		_economyService.OnLoaded();

		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;

		var seed1 = new SeedModel("1", new CropData())
		{
			Seasons = Seasons.Summer,
		};
		var seed2 = new SeedModel("2", new CropData())
		{
			Seasons = Seasons.Fall,
		};
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("1")).Returns(seed1);
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("2")).Returns(seed2);
		
		_economyService.Reset(true, true, Seasons.Summer);

		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(777));
			Assert.That(cat1Items[1].Supply, Is.EqualTo(777));
			Assert.That(cat2Items[0].Supply, Is.EqualTo(777));
			Assert.That(cat2Items[1].Supply, Is.EqualTo(777));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(26));
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(25));
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27));
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
		
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(2));
	}
	
	[Test]
	public void ShouldResetWithCurrentSeasonsFish()
	{
		_economyService.OnLoaded();

		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 12;
		cat1Items[0].DailyDelta = 12;
		cat1Items[1].Supply = 12;
		cat1Items[1].DailyDelta = 12;
		cat2Items[0].Supply = 12;
		cat2Items[0].DailyDelta = 12;
		cat2Items[1].Supply = 12;
		cat2Items[1].DailyDelta = 12;

		var fish1 = new FishModel("1","a/b/c/d/e/f/Spring")
		{
			Seasons = Seasons.Summer,
		};
		var fish2 = new FishModel("2","a/b/c/d/e/f/Spring")
		{
			Seasons = Seasons.Fall,
		};
		_mockFishService.Setup(m => m.GetFishModelFromModelId("1")).Returns(fish1);
		_mockFishService.Setup(m => m.GetFishModelFromModelId("2")).Returns(fish2);
		
		_economyService.Reset(true, true, Seasons.Summer);

		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(777));
			Assert.That(cat1Items[1].Supply, Is.EqualTo(777));
			Assert.That(cat2Items[0].Supply, Is.EqualTo(777));
			Assert.That(cat2Items[1].Supply, Is.EqualTo(777));
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(26));
			Assert.That(cat1Items[1].DailyDelta, Is.EqualTo(25));
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(27));
			Assert.That(cat2Items[1].DailyDelta, Is.EqualTo(27));
		});
		
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(2));
	}
	
	[Test]
	public void ShouldAdvanceOneDay()
	{
		_economyService.OnLoaded();
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 100;
		cat1Items[0].DailyDelta = 5;
		cat1Items[1].Supply = 100;
		cat1Items[1].DailyDelta = 10;
		cat2Items[0].Supply = 100;
		cat2Items[0].DailyDelta = 15;
		cat2Items[1].Supply = 100;
		cat2Items[1].DailyDelta = 20;
		
		_economyService.AdvanceOneDay();
		
		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(105));
			Assert.That(cat1Items[1].Supply, Is.EqualTo(110));
			Assert.That(cat2Items[0].Supply, Is.EqualTo(115));
			Assert.That(cat2Items[1].Supply, Is.EqualTo(120));
		});
		
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.IsAny<EconomyModelMessage>()), Times.Exactly(1));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(2));
	}
	
	[Test]
	public void ShouldNotAdvanceOneDayIfClient()
	{
		_economyService.OnLoaded();
		
		HarmonyFarmer.IsMainPlayerDictionary.Clear();
		HarmonyFarmer.IsMainPlayerDictionary.Add(_player, false);
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);

		cat1Items[0].Supply = 100;
		cat1Items[0].DailyDelta = 5;
		cat1Items[1].Supply = 100;
		cat1Items[1].DailyDelta = 10;
		cat2Items[0].Supply = 100;
		cat2Items[0].DailyDelta = 15;
		cat2Items[1].Supply = 100;
		cat2Items[1].DailyDelta = 20;
		
		_economyService.AdvanceOneDay();
		
		Assert.Multiple(() =>
		{
			Assert.That(cat1Items[0].Supply, Is.EqualTo(100));
			Assert.That(cat1Items[1].Supply, Is.EqualTo(100));
			Assert.That(cat2Items[0].Supply, Is.EqualTo(100));
			Assert.That(cat2Items[1].Supply, Is.EqualTo(100));
		});
		
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.IsAny<EconomyModelMessage>()), Times.Exactly(0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(1));
	}

	[Test]
	public void ShouldGetPriceForNormalItem()
	{
		_economyService.OnLoaded();
		
		var cat1Items = _economyService.GetItemsForCategory(1);

		cat1Items[0].Supply = 0;
		
		cat1Items[0].UpdateMultiplier();

		var sellable = new Object("1", 1);

		Assert.Multiple(() =>
		{ 
			Assert.That(_economyService.GetPrice(sellable, 100), Is.EqualTo(130)); 
			Assert.That(_economyService.GetPrice(sellable, 1000), Is.EqualTo(1300));
		});

		cat1Items[0].Supply = 1000;
		
		Assert.Multiple(() =>
		{ 
			Assert.That(_economyService.GetPrice(sellable, 100), Is.EqualTo(130)); 
			Assert.That(_economyService.GetPrice(sellable, 1000), Is.EqualTo(1300));
		});
		
		cat1Items[0].UpdateMultiplier();
		
		Assert.Multiple(() =>
		{ 
			Assert.That(_economyService.GetPrice(sellable, 100), Is.EqualTo(20)); 
			Assert.That(_economyService.GetPrice(sellable, 1000), Is.EqualTo(200));
		});
	}

	[Test]
	public void ShouldGetPriceForArtisanItemWithNoBase()
	{
		HarmonyObject.ObjectIdCategoryMapping.Clear();

		Game1.objectData = new Dictionary<string, ObjectData>(new[]
		{
			GenerateObjectData("1", Object.artisanGoodsCategory),
			GenerateObjectData("2", 1),
			GenerateObjectData("3", 2),
			GenerateObjectData("4", 2),
		});
		
		ConfigModel.Instance = new ConfigModel()
		{
			ValidCategories = [1, 2, 3, 4, 5, Object.artisanGoodsCategory],
		};
		
		_economyService.OnLoaded();
		
		var catArtisanItems = _economyService.GetItemsForCategory(Object.artisanGoodsCategory);
		var cat1Items = _economyService.GetItemsForCategory(1);

		catArtisanItems[0].Supply = 1000;
		cat1Items[0].Supply = 0;
		
		catArtisanItems[0].UpdateMultiplier();
		cat1Items[0].UpdateMultiplier();

		var sellable = new Object("1", 1);

		Assert.Multiple(() =>
		{ 
			Assert.That(_economyService.GetPrice(sellable, 100), Is.EqualTo(20)); 
			Assert.That(_economyService.GetPrice(sellable, 1000), Is.EqualTo(200));
		});
	}

	[Test]
	public void ShouldGetPriceForArtisanItemWithBase()
	{
		HarmonyObject.ObjectIdCategoryMapping.Clear();

		Game1.objectData = new Dictionary<string, ObjectData>(new[]
		{
			GenerateObjectData("1", Object.artisanGoodsCategory),
			GenerateObjectData("2", 1),
			GenerateObjectData("3", 2),
			GenerateObjectData("4", 2),
		});
		
		ConfigModel.Instance = new ConfigModel()
		{
			ValidCategories = [1, 2, 3, 4, 5, Object.artisanGoodsCategory],
		};
		
		HarmonyObject.ObjectIdToPriceMapping.Add("2", 10);
		
		_economyService.OnLoaded();
		
		var catArtisanItems = _economyService.GetItemsForCategory(Object.artisanGoodsCategory);
		var cat1Items = _economyService.GetItemsForCategory(1);

		catArtisanItems[0].Supply = 1000;
		cat1Items[0].Supply = 0;
		
		catArtisanItems[0].UpdateMultiplier();
		cat1Items[0].UpdateMultiplier();

		var sellable = new Object("1", 1)
		{
			preservedParentSheetIndex =
			{
				Value = "2",
			},
		};

		Assert.Multiple(() =>
		{ 
			Assert.That(_economyService.GetPrice(sellable, 100), Is.EqualTo(130)); 
			Assert.That(_economyService.GetPrice(sellable, 1000), Is.EqualTo(1300));
		});
	}
	
	[Test]
	public void ShouldAdjustSupplyForNormalItem(
		[Values] bool isClient,
		[Values] bool shouldNotifyPeers
	)
	{
		_economyService.OnLoaded();
		
		HarmonyFarmer.IsMainPlayerDictionary.Clear();
		HarmonyFarmer.IsMainPlayerDictionary.Add(_player, !isClient);
		
		var cat1Items = _economyService.GetItemsForCategory(1);

		cat1Items[0].Supply = 0;
		
		cat1Items[0].UpdateMultiplier();

		var sellable = new Object("1", 1);
		
		_economyService.AdjustSupply(sellable, 100, shouldNotifyPeers);
		
		cat1Items = _economyService.GetItemsForCategory(1);
		
		Assert.That(cat1Items[0].Supply, Is.EqualTo(100));
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.Is<SupplyAdjustedMessage>(e => e.ObjectId == "1" && e.Amount == 100)), Times.Exactly(shouldNotifyPeers ? 1 : 0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(isClient ? 1 : 2));
	}
	
	[Test]
	public void ShouldAdjustSupplyForArtisanItemWithNoBase(
		[Values] bool isClient,
		[Values] bool shouldNotifyPeers
	)
	{
		HarmonyObject.ObjectIdCategoryMapping.Clear();

		Game1.objectData = new Dictionary<string, ObjectData>(new[]
		{
			GenerateObjectData("1", Object.artisanGoodsCategory),
			GenerateObjectData("2", 1),
			GenerateObjectData("3", 2),
			GenerateObjectData("4", 2),
		});
		
		ConfigModel.Instance = new ConfigModel()
		{
			ValidCategories = [1, 2, 3, 4, 5, Object.artisanGoodsCategory],
		};
		
		_economyService.OnLoaded();
		
		HarmonyFarmer.IsMainPlayerDictionary.Clear();
		HarmonyFarmer.IsMainPlayerDictionary.Add(_player, !isClient);
		
		var catArtisanItems = _economyService.GetItemsForCategory(Object.artisanGoodsCategory);
		var cat1Items = _economyService.GetItemsForCategory(1);

		catArtisanItems[0].Supply = 0;
		cat1Items[0].Supply = 0;

		var sellable = new Object("1", 1);
		
		_economyService.AdjustSupply(sellable, 100, shouldNotifyPeers);
		
		cat1Items = _economyService.GetItemsForCategory(1);
		Assert.Multiple(() =>
		{ 
			Assert.That(catArtisanItems[0].Supply, Is.EqualTo(100)); 
			Assert.That(cat1Items[0].Supply, Is.EqualTo(0));
		});
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.Is<SupplyAdjustedMessage>(e => e.ObjectId == "1" && e.Amount == 100)), Times.Exactly(shouldNotifyPeers ? 1 : 0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(isClient ? 1 : 2));
	}
	
	[Test]
	public void ShouldAdjustSupplyForArtisanItemWithBase(
		[Values] bool isClient,
		[Values] bool shouldNotifyPeers
	)
	{
		HarmonyObject.ObjectIdCategoryMapping.Clear();

		Game1.objectData = new Dictionary<string, ObjectData>(new[]
		{
			GenerateObjectData("1", Object.artisanGoodsCategory),
			GenerateObjectData("2", 1),
			GenerateObjectData("3", 2),
			GenerateObjectData("4", 2),
		});
		
		ConfigModel.Instance = new ConfigModel()
		{
			ValidCategories = [1, 2, 3, 4, 5, Object.artisanGoodsCategory],
		};
		
		_economyService.OnLoaded();
		
		HarmonyFarmer.IsMainPlayerDictionary.Clear();
		HarmonyFarmer.IsMainPlayerDictionary.Add(_player, !isClient);
		
		var catArtisanItems = _economyService.GetItemsForCategory(Object.artisanGoodsCategory);
		var cat1Items = _economyService.GetItemsForCategory(1);

		catArtisanItems[0].Supply = 0;
		cat1Items[0].Supply = 0;

		var sellable = new Object("1", 1)
		{
			preservedParentSheetIndex =
			{
				Value = "2",
			},
		};
		
		_economyService.AdjustSupply(sellable, 100, shouldNotifyPeers);
		
		cat1Items = _economyService.GetItemsForCategory(1);
		Assert.Multiple(() =>
		{ 
			Assert.That(catArtisanItems[0].Supply, Is.EqualTo(0)); 
			Assert.That(cat1Items[0].Supply, Is.EqualTo(100));
		});
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.Is<SupplyAdjustedMessage>(e => e.ObjectId == "2" && e.Amount == 100)), Times.Exactly(shouldNotifyPeers ? 1 : 0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(isClient ? 1 : 2));
	}

	[Test]
	public void ShouldSayNonSeasonItemIsValidForEverySeason()
	{
		_economyService.OnLoaded();

		var cat1Items = _economyService.GetItemsForCategory(1);
		
		Assert.Multiple(() =>
		{
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Spring), Is.True);
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Summer), Is.True);
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Fall), Is.True);
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Winter), Is.True);
		});
	}
	
	[TestCase(Seasons.Spring)]
	[TestCase(Seasons.Summer)]
	[TestCase(Seasons.Fall)]
	[TestCase(Seasons.Winter)]
	public void ShouldSayCropIsValidForSeedSeason(Seasons seedSeason)
	{
		_economyService.OnLoaded();

		var seedModel = new SeedModel("s1", new CropData())
		{
			Seasons = seedSeason
		};
		
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("1")).Returns(seedModel);

		var cat1Items = _economyService.GetItemsForCategory(1);
		
		Assert.Multiple(() =>
		{
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Spring), Is.EqualTo(seedSeason == Seasons.Spring));
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Summer), Is.EqualTo(seedSeason == Seasons.Summer));
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Fall), Is.EqualTo(seedSeason == Seasons.Fall));
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Winter), Is.EqualTo(seedSeason == Seasons.Winter));
		});
	}
	
	[TestCase(Seasons.Spring)]
	[TestCase(Seasons.Summer)]
	[TestCase(Seasons.Fall)]
	[TestCase(Seasons.Winter)]
	[TestCase(Seasons.Spring | Seasons.Summer)]
	[TestCase(Seasons.Summer | Seasons.Fall | Seasons.Winter)]
	public void ShouldSayItemIsValidForFishSeason(Seasons fishSeason)
	{
		_economyService.OnLoaded();

		var fishModel = new FishModel("fish","a/b/c/d/e/f/Spring")
		{
			Seasons = fishSeason,
		};
		_mockFishService.Setup(m => m.GetFishModelFromModelId("1")).Returns(fishModel);
		var cat1Items = _economyService.GetItemsForCategory(1);
		
		Assert.Multiple(() =>
		{
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Spring), Is.EqualTo(fishSeason.HasFlag(Seasons.Spring)));
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Summer), Is.EqualTo(fishSeason.HasFlag(Seasons.Summer)));
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Fall), Is.EqualTo(fishSeason.HasFlag(Seasons.Fall)));
			Assert.That(_economyService.ItemValidForSeason(cat1Items[0], Seasons.Winter), Is.EqualTo(fishSeason.HasFlag(Seasons.Winter)));
		});
	}

	[Test]
	public void ShouldGetPricePerDayForNonCrop()
	{
		_economyService.OnLoaded();
		
		var cat1Items = _economyService.GetItemsForCategory(1);
		
		Assert.That(_economyService.GetPricePerDay(cat1Items[0]), Is.EqualTo(-1));
	}
	
	[Test]
	public void ShouldGetPricePerDayForCrop()
	{
		_economyService.OnLoaded();
		
		var seedModel = new SeedModel("s1", new CropData())
		{
			DaysToGrow = 10
		};

		var itemModel = new ItemModel("200")
		{
			Supply = 0,
			DailyDelta = 0
		};
		
		_mockSeedService.Setup(m => m.GetSeedModelFromModelId("200")).Returns(seedModel);
		
		Assert.That(_economyService.GetPricePerDay(itemModel), Is.EqualTo(20));
	}

	[Test]
	public void ShouldForwardItemModelFromSeed()
	{
		var itemModel = new ItemModel("seed");
		_mockSeedService.Setup(m => m.GetItemModelFromSeedId("seed")).Returns(itemModel);
		
		Assert.That(_economyService.GetItemModelFromSeed("seed"), Is.EqualTo(itemModel));
	}

	[Test]
	public void ShouldConsolidateEconomyModelWhenLoadingWithMismatches()
	{
		HarmonyObject.ObjectIdCategoryMapping.Clear();
		Game1.objectData = new Dictionary<string, ObjectData>(new[]
		{
			GenerateObjectData("1", 1),
			GenerateObjectData("22", 1),
			GenerateObjectData("3", 3),
			GenerateObjectData("4", 2),
		});
		
		var model = new EconomyModel(new Dictionary<int, Dictionary<string, ItemModel>>
			{
				{1, new Dictionary<string, ItemModel>()
				{
					{"1", new ItemModel("1"){ DailyDelta = 11, Supply = 110}},
					{"2", new ItemModel("2"){ DailyDelta = 12, Supply = 120}},
				}},
				{2, new Dictionary<string, ItemModel>()
				{
					{"3", new ItemModel("3"){ DailyDelta = 21, Supply = 210}},
					{"4", new ItemModel("4"){ DailyDelta = 22, Supply = 220}},
				}},
			}
		);
		HarmonyObject.CategoryIdToNameMapping.Add(3, "Cat3");
		
		_mockDataHelper.Setup(m => m.ReadSaveData<EconomyModel>(EconomyModel.ModelKey))
			.Returns(model);

		_economyService.OnLoaded();
		
		_mockMultiplayerService.Verify(m => m.SendMessageToPeers(It.IsAny<RequestEconomyModelMessage>()), Times.Exactly(0));
		_mockDataHelper.Verify(m => m.WriteSaveData(EconomyModel.ModelKey, It.IsAny<EconomyModel>()), Times.Exactly(1));
		_mockFishService.Verify(m => m.GenerateFishMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		_mockSeedService.Verify(m => m.GenerateSeedMapping(It.IsAny<EconomyModel>()), Times.Exactly(1));
		Assert.That(_economyService.Loaded, Is.True);

		var categories = _economyService.GetCategories();
		
		Assert.That(categories, Has.Count.EqualTo(3));
		Assert.Multiple(() =>
		{ 
			Assert.That(categories[1], Is.EqualTo("Cat1")); 
			Assert.That(categories[2], Is.EqualTo("Cat2"));
			Assert.That(categories[3], Is.EqualTo("Cat3"));
		});

		var cat1Items = _economyService.GetItemsForCategory(1);
		var cat2Items = _economyService.GetItemsForCategory(2);
		var cat3Items = _economyService.GetItemsForCategory(3);
		
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items, Has.Length.EqualTo(2)); 
			Assert.That(cat2Items, Has.Length.EqualTo(1));
			Assert.That(cat3Items, Has.Length.EqualTo(1));
		});
		Assert.Multiple(() =>
		{ 
			Assert.That(cat1Items[0].ObjectId, Is.EqualTo("1")); 
			Assert.That(cat2Items[0].ObjectId, Is.EqualTo("4")); 
			Assert.That(cat3Items[0].ObjectId, Is.EqualTo("3"));
			Assert.That(cat1Items[1].ObjectId, Is.EqualTo("22")); 
			
			Assert.That(cat1Items[0].Supply, Is.EqualTo(110)); 
			Assert.That(cat2Items[0].Supply, Is.EqualTo(220)); 
			Assert.That(cat3Items[0].Supply, Is.EqualTo(210));
			Assert.That(cat1Items[1].Supply, Is.Not.EqualTo(120)); 
			Assert.That(cat1Items[1].Supply, Is.Not.EqualTo(0)); 
			
			Assert.That(cat1Items[0].DailyDelta, Is.EqualTo(11)); 
			Assert.That(cat2Items[0].DailyDelta, Is.EqualTo(22)); 
			Assert.That(cat3Items[0].DailyDelta, Is.EqualTo(21)); 
			Assert.That(cat1Items[1].DailyDelta, Is.Not.EqualTo(12)); 
			Assert.That(cat1Items[1].DailyDelta, Is.Not.EqualTo(0)); 
		});
	}

	[Test]
	public void ShouldGetConsolidatedItem()
	{
		Game1.objectData = new Dictionary<string, ObjectData>(new[]
		{
			GenerateObjectData("176", 1),
			GenerateObjectData("174", 1),
			GenerateObjectData("442", 2),
			GenerateObjectData("307", 2),
			GenerateObjectData("1", 2),
		});
		
		_economyService = new EconomyService
		(
			_mockModHelper.Object,
			_mockMonitor.Object,
			_mockMultiplayerService.Object,
			_mockFishService.Object,
			_mockSeedService.Object,
			_mockArtisanService.Object,
			_mockNormalDistributionService.Object,
			_mockUpdateFrequencyService.Object
		);
		
		_economyService.OnLoaded();

		var itemModel = new ItemModel("2");
		var equivalentModel = new ItemModel("174");
		var artisanModel = new ItemModel("307");
		Assert.Multiple(() =>
		{ 
			Assert.That(_economyService.GetConsolidatedItem(itemModel), Is.EqualTo(itemModel)); 
			Assert.That(_economyService.GetConsolidatedItem(equivalentModel).ObjectId, Is.EqualTo("176")); 
			Assert.That(_economyService.GetConsolidatedItem(artisanModel).ObjectId, Is.EqualTo("442"));
		});
 }

	private static IEnumerable<TestCaseData> ShouldGetBreakEvenSupplyTestCases()
	{
		yield return new TestCaseData(1000, .2m, 1m, 0f);
		yield return new TestCaseData(1000, .2m, .2m, -1f);
		yield return new TestCaseData(1000, 1.1m, 2.0m, -1f);
		yield return new TestCaseData(1000, .2m, .9m, -1f);
		yield return new TestCaseData(1000, .2m, 1.3m, 272.72f);
		yield return new TestCaseData(10000, .2m, 1.3m, 2727.27f);
		yield return new TestCaseData(1000, .1m, 1.8m, 470.58f);
	}
	
	[Test, TestCaseSource(nameof(ShouldGetBreakEvenSupplyTestCases))]
	public void ShouldGetBreakEvenSupply
	(
		int maxCalculatedSupply,
		decimal minPercentage,
		decimal maxPercentage,
		float expectedSupply
		)
	{
		ConfigModel.Instance.MaxCalculatedSupply = maxCalculatedSupply;
		ConfigModel.Instance.MinPercentage = minPercentage;
		ConfigModel.Instance.MaxPercentage = maxPercentage;

		Assert.That(_economyService.GetBreakEvenSupply(), Is.EqualTo(expectedSupply).Within(.1f));
	}

	private static KeyValuePair<string, ObjectData> GenerateObjectData(string itemId, int category)
	{
		HarmonyObject.ObjectIdCategoryMapping.TryAdd(itemId, category);
		return new KeyValuePair<string, ObjectData>(itemId, new ObjectData { Category = category });
	}
}
