using fse.core.models;
using fse.core.services;
using Moq;
using StardewModdingAPI;
using StardewValley.GameData.Machines;

namespace Tests.services;

public class ArtisanServiceTests
{
	private Mock<IMonitor> _mockMonitor;
	private Mock<IModHelper> _mockHelper;
	private Mock<IGameContentHelper> _gameContentHelper;
	private ArtisanService _artisanService;
	private EconomyModel _economyModel;

	[SetUp]
	public void Setup()
	{
		_mockMonitor = new Mock<IMonitor>();
		_mockHelper = new Mock<IModHelper>();
		_gameContentHelper = new Mock<IGameContentHelper>();
		_economyModel = new EconomyModel
		{
			CategoryEconomies = new Dictionary<int, Dictionary<string, ItemModel>>
			{
				{
					1, new Dictionary<string, ItemModel>
					{
						{ "1", new ItemModel { ObjectId = "1" } },
						{ "2", new ItemModel { ObjectId = "2" } },
						{ "3", new ItemModel { ObjectId = "3" } },
						{ "4", new ItemModel { ObjectId = "4" } },
					}
				},
			},
		};
		
		_mockHelper.SetupGet(m => m.GameContent).Returns(_gameContentHelper.Object);

		_artisanService = new ArtisanService(_mockMonitor.Object, _mockHelper.Object);
	}

	private void GenerateMachineData(params (string output, string input)[] mappings)
	{
		var dict = new Dictionary<string, MachineData>
		{
			{
				"machine1", new MachineData
				{
					OutputRules =
					[
						..mappings.Select(m => new MachineOutputRule
						{
							OutputItem = new[]
							{
								new MachineItemOutput { ItemId = m.output },
							}.ToList(),
							Triggers = new[]
							{
								new MachineOutputTriggerRule { RequiredItemId = m.input },
							}.ToList(),
						}).ToArray(),
					],
				}
			},
		};

		_gameContentHelper.Setup(m => m.Load<Dictionary<string, MachineData>>("Data\\Machines")).Returns(dict);
	}

	[Test]
	public void ShouldGenerateBasicMapping()
	{
		// Arrange
		GenerateMachineData(("2", "1"));

		// Act
		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("2");

		// Assert
		Assert.That(result.ObjectId, Is.EqualTo("1"));
	}

	[Test]
	public void ShouldHandleNullMappingGracefully()
	{
		_artisanService.GenerateArtisanMapping(_economyModel);
		// Act
		var result = _artisanService.GetBaseFromArtisanGood("1");

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public void ShouldIgnoreNonExistentItems()
	{
		// Arrange
		GenerateMachineData(
			("2", "1"),
			("999", "998") // Non-existent items
		);

		// Act
		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("999");

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public void ShouldHandleMultiStepArtisanChain()
	{
		// Arrange
		GenerateMachineData(
			("3", "2"),
			("2", "1")
		);

		// Act
		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("3");

		// Assert
		Assert.That(result.ObjectId, Is.EqualTo("1"));
	}

	[Test]
	public void ShouldBreakCyclesAndLogWarning()
	{
		// Arrange
		GenerateMachineData(
			("2", "1"),
			("3", "2"),
			("1", "3") // Creates cycle
		);

		// Act
		_artisanService.GenerateArtisanMapping(_economyModel);
		var result1 = _artisanService.GetBaseFromArtisanGood("1");
		var result2 = _artisanService.GetBaseFromArtisanGood("2");
		var result3 = _artisanService.GetBaseFromArtisanGood("3");

		Assert.Multiple(() =>
		{
			Assert.That(result1.ObjectId, Is.EqualTo("2"));
			Assert.That(result2, Is.Null);
			Assert.That(result3.ObjectId, Is.EqualTo("2"));
		});

		_mockMonitor.Verify(
			m => m.LogOnce(
				It.Is<string>(s => s.Contains("cycle detected")),
				LogLevel.Warn
			),
			Times.Once
		);
		_mockMonitor.Verify(
			m => m.LogOnce(
				It.Is<string>(s => s.Contains("2 < 1 < 3 < 2")),
				LogLevel.Warn
			),
			Times.Once
		);
	}

	[Test]
	public void ShouldIgnoreSelfReferencingItems()
	{
		// Arrange
		GenerateMachineData(("1", "1"));

		// Act
		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("1");

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public void ShouldHandleEmptyMachineData()
	{
		// Arrange
		var dict = new Dictionary<string, MachineData>();

		_gameContentHelper.Setup(m => m.Load<Dictionary<string, MachineData>>("Data\\Machines")).Returns(dict);

		// Act
		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("1");

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public void ShouldHandleMachineWithNoOutputRules()
	{
		// Arrange
		var dict = new Dictionary<string, MachineData>
		{
			{
				"machine1", new MachineData
				{
					OutputRules =
					[
					],
				}
			},
		};

		_gameContentHelper.Setup(m => m.Load<Dictionary<string, MachineData>>("Data\\Machines")).Returns(dict);

		// Act
		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("1");

		// Assert
		Assert.That(result, Is.Null);
	}
}