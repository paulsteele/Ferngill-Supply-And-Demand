using fse.core.models;
using StardewValley;
using StardewValley.GameData.Crops;

namespace Tests.models;

public class SeedModelTest
{
	private const string ObjectId = "TestId";
	private CropData _cropEntry;

	[SetUp]
	public void Setup()
	{
		_cropEntry = new CropData
		{
			HarvestItemId = "TestCropId",
			DaysInPhase = [1, 2, 3],
		};
	}

	[Test]
	public void Test_ObjectId()
	{
		var model = new SeedModel(ObjectId, _cropEntry);
		Assert.That(model.ObjectId, Is.EqualTo(ObjectId));
	}

	[Test]
	public void ShouldSetCropId()
	{
		var model = new SeedModel(ObjectId, _cropEntry);
		Assert.That(model.CropId, Is.EqualTo(_cropEntry.HarvestItemId));
	}

	[Test]
	public void ShouldSetDaysToGrow()
	{
		var model = new SeedModel(ObjectId, _cropEntry);
		Assert.That(model.DaysToGrow, Is.EqualTo(6));
	}

	[TestCase(new[] {Season.Spring}, Seasons.Spring)]
	[TestCase(new[] {Season.Spring, Season.Summer}, Seasons.Spring | Seasons.Summer)]
	[TestCase(new[] {Season.Spring, Season.Fall}, Seasons.Spring | Seasons.Fall)]
	[TestCase(new[] {Season.Spring, Season.Winter}, Seasons.Spring | Seasons.Winter)]
	[TestCase(new[] {Season.Spring, Season.Summer, Season.Fall, Season.Winter}, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter)]
	public void ShouldSetSseasons(Season[] seasonList, Seasons expectedSeasons)
	{
		_cropEntry = new CropData()
		{
			HarvestItemId = "TestCropId",
			DaysInPhase = [1, 2, 3],
			Seasons = seasonList.ToList(),
		};
		
		var model = new SeedModel(ObjectId, _cropEntry);
		Assert.That(model.Seasons, Is.EqualTo(expectedSeasons));
	}
}