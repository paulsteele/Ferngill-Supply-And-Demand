using fse.core.models;

namespace Tests.models;

[TestFixture]
public class FishModelTests
{
	[TestCase("spring", Seasons.Spring)]
	[TestCase("summer", Seasons.Summer)]
	[TestCase("fall", Seasons.Fall)]
	[TestCase("winter", Seasons.Winter)]
	[TestCase("spring summer", Seasons.Spring | Seasons.Summer)]
	[TestCase("fall winter", Seasons.Fall | Seasons.Winter)]
	[TestCase("spring summer fall winter", Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter)]
	[TestCase("", 0)]
	public void ShouldParseSeasonsAppropriately(string seasonString, Seasons expectedSeasons)
	{
		var fishModel = new FishModel("1", $"a/b/c/d/e/f/{seasonString}");
			
		Assert.Multiple(() => 
		{
			Assert.That(fishModel.ObjectId, Is.Not.Empty);
			Assert.That(fishModel.Seasons, Is.EqualTo(expectedSeasons)); 
		});
	}
}