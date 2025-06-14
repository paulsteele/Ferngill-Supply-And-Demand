using System.Text;
using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using StardewValley;

namespace Tests.services;

[TestFixture]
public class UpdateFrequencyServiceTests
{
	private UpdateFrequencyService _service;

	[SetUp]
	public void Setup()
	{
		_service = new UpdateFrequencyService();
	}

	[TearDown]
	public void TearDown()
	{
		ConfigModel.Instance = new ConfigModel();
	}

	private static IEnumerable<TestCaseData> UpdateFrequencyTestCases()
	{
		(
			UpdateFrequency updateFrequency,
			int customUpdateFrequency,
			DayModel dayModel,
			bool expectedUpdate
		)[] cases =
		[
			// Yearly
			(UpdateFrequency.Yearly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), false),
			(UpdateFrequency.Yearly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 27), false),
			(UpdateFrequency.Yearly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall, 28),false),
			(UpdateFrequency.Yearly, 1, new DayModel(2, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), false),
			(UpdateFrequency.Yearly, 1, new DayModel(2, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 27), false),
			(UpdateFrequency.Yearly, 1, new DayModel(2, Seasons.Spring | Seasons.Summer | Seasons.Fall, 28),false),
			// Seasonally
			(UpdateFrequency.Seasonally, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), false),
			(UpdateFrequency.Seasonally, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 27), false),
			(UpdateFrequency.Seasonally, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 28), true),
			// Weekly
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), false),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 6), false),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 7), true),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 8), false),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 13), false),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 14), true),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 15), false),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 20), false),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 21), true),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 22), false),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 27), false),
			(UpdateFrequency.Weekly, 1, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 28), true),
			(UpdateFrequency.Weekly, 1, new DayModel(2, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), false),
			(UpdateFrequency.Weekly, 1, new DayModel(3, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 6), false),
			(UpdateFrequency.Weekly, 1, new DayModel(4, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 7), true),
			(UpdateFrequency.Weekly, 1, new DayModel(5, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 8), false),
			(UpdateFrequency.Weekly, 1, new DayModel(6, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 13), false),
			(UpdateFrequency.Weekly, 1, new DayModel(7, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 14), true),
			(UpdateFrequency.Weekly, 1, new DayModel(8, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 15), false),
			(UpdateFrequency.Weekly, 1, new DayModel(9, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 20), false),
			(UpdateFrequency.Weekly, 1, new DayModel(10, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 21), true),
			(UpdateFrequency.Weekly, 1, new DayModel(11, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 22), false),
			(UpdateFrequency.Weekly, 1, new DayModel(12, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 27), false),
			(UpdateFrequency.Weekly, 1, new DayModel(13, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 28), true),
			// Daily
			(UpdateFrequency.Daily, 2, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), true),
			(UpdateFrequency.Daily, 2, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 2), true),
			(UpdateFrequency.Daily, 2, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 3), true),
			(UpdateFrequency.Daily, 2, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 28), true),
			(UpdateFrequency.Daily, 2, new DayModel(7, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), true),
			(UpdateFrequency.Daily, 2, new DayModel(8, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 2), true),
			(UpdateFrequency.Daily, 2, new DayModel(9, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 3), true),
			(UpdateFrequency.Daily, 2, new DayModel(10, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 28), true),
			// Custom
			(UpdateFrequency.Custom, 2, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), false),
			(UpdateFrequency.Custom, 2, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 2), true),
			(UpdateFrequency.Custom, 2, new DayModel(1, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 3), false),
			(UpdateFrequency.Custom, 2, new DayModel(2, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), false),
			(UpdateFrequency.Custom, 2, new DayModel(2, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 2), true),
			(UpdateFrequency.Custom, 2, new DayModel(2, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 3), false),
			(UpdateFrequency.Custom, 2, new DayModel(3, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 1), false),
			(UpdateFrequency.Custom, 2, new DayModel(3, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 2), true),
			(UpdateFrequency.Custom, 2, new DayModel(3, Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter, 3), false),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Spring,  28), false),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Summer,  1), false),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Summer,  2), true),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Summer,  3), false),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Fall,  3), false),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Fall,  4), true),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Fall,  5), false),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Winter,  5), false),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Winter,  6), true),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Winter,  7), false),
			(UpdateFrequency.Custom, 30, new DayModel(1, Seasons.Winter,  28), false),
			(UpdateFrequency.Custom, 30, new DayModel(2, Seasons.Spring,  7), false),
			(UpdateFrequency.Custom, 30, new DayModel(2, Seasons.Spring,  8), true),
			(UpdateFrequency.Custom, 30, new DayModel(2, Seasons.Spring,  9), false),
			(UpdateFrequency.Custom, 113, new DayModel(1, Seasons.Spring,  1), false),
			(UpdateFrequency.Custom, 113, new DayModel(1, Seasons.Winter,  28), false),
			(UpdateFrequency.Custom, 113, new DayModel(2, Seasons.Spring,  1), true),
			(UpdateFrequency.Custom, 113, new DayModel(2, Seasons.Spring,  2), false),
			(UpdateFrequency.Custom, 113, new DayModel(3, Seasons.Spring,  1), false),
			(UpdateFrequency.Custom, 113, new DayModel(3, Seasons.Spring,  2), true),
			(UpdateFrequency.Custom, 113, new DayModel(3, Seasons.Spring,  3), false),
		];

		foreach (var testCase in cases)
		{
			foreach (var season in GetIndividualFlags(testCase.dayModel.Season))
			{
				var dayModel = testCase.dayModel with { Season = season };
				var data = new TestCaseData
				(
					testCase.updateFrequency,
					testCase.customUpdateFrequency,
					dayModel,
					testCase.expectedUpdate
				);
		
				var builder = new StringBuilder();
				builder.Append("When frequency { ");
				builder.Append(testCase.updateFrequency == UpdateFrequency.Custom ? testCase.customUpdateFrequency : testCase.updateFrequency);
				builder.Append(" } and day model ");
				builder.Append(dayModel);
				builder.Append(" then update == ");
				builder.Append(testCase.expectedUpdate);

				data.SetName(builder.ToString());

				yield return data;
			}
		}
	}

	[TestCaseSource(nameof(UpdateFrequencyTestCases))]
	public void ShouldDetermineSupplyUpdateFrequency(
		UpdateFrequency updateFrequency,
		int customUpdateFrequency,
		DayModel dayModel,
		bool expectedShouldUpdate
	)
	{
		ConfigModel.Instance.SupplyUpdateFrequency = updateFrequency;
		ConfigModel.Instance.CustomSupplyUpdateFrequency = customUpdateFrequency;

		var result = _service.GetUpdateFrequencyInformation(dayModel);
		Assert.That(result.ShouldUpdateSupply, Is.EqualTo(expectedShouldUpdate)); 
	}
	
	[TestCaseSource(nameof(UpdateFrequencyTestCases))]
	public void ShouldDetermineDeltaUpdateFrequency(
		UpdateFrequency updateFrequency,
		int customUpdateFrequency,
		DayModel dayModel,
		bool expectedShouldUpdate
	)
	{
		ConfigModel.Instance.DeltaUpdateFrequency = updateFrequency;
		ConfigModel.Instance.CustomDeltaUpdateFrequency = customUpdateFrequency;

		var result = _service.GetUpdateFrequencyInformation(dayModel);
		Assert.That(result.ShouldUpdateDelta, Is.EqualTo(expectedShouldUpdate)); 
	}

	[TestCase(Season.Spring, 27, Seasons.Spring)]
	[TestCase(Season.Spring, 28, Seasons.Summer)]
	[TestCase(Season.Summer, 27, Seasons.Summer)]
	[TestCase(Season.Summer, 28, Seasons.Fall)]
	[TestCase(Season.Fall, 27, Seasons.Fall)]
	[TestCase(Season.Fall, 28, Seasons.Winter)]
	[TestCase(Season.Winter, 27, Seasons.Winter)]
	[TestCase(Season.Winter, 28, Seasons.Spring)]
	public void ShouldReturnCorrectSeason(Season inputSeason, int dayOfMonth, Seasons expectedSeason)
	{
		Game1.season = inputSeason;
		
		var dayModel = new DayModel(1, SeasonHelper.GetCurrentSeason(), dayOfMonth);
		var result = _service.GetUpdateFrequencyInformation(dayModel);
		Assert.That(result.UpdateSeason, Is.EqualTo(expectedSeason));
	}

	private static IEnumerable<T> GetIndividualFlags<T>(T flags) where T : Enum => 
		from Enum value in Enum.GetValues(flags.GetType()) 
		where !value.Equals(default(T)) 
		where flags.HasFlag(value) 
		select (T)value;
}
