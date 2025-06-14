using System.Text;
using fse.core.models;
using fse.core.services;

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
			UpdateFrequency supplyUpdateFrequency,
			int customSupplyUpdateFrequency,
			UpdateFrequency deltaUpdateFrequency,
			int customDeltaUpdateFrequency,
			int year,
			Seasons season,
			int dayOfMonth,
			bool expectedSupplyUpdate,
			bool expectedDeltaUpdate
		)[] cases =
		[
			(UpdateFrequency.Yearly, 1, UpdateFrequency.Yearly, 1, 1, Seasons.Spring, 1, false, false),
			(UpdateFrequency.Yearly, 1, UpdateFrequency.Yearly, 1, 1, Seasons.Fall, 28, false, false),
			(UpdateFrequency.Yearly, 1, UpdateFrequency.Yearly, 1, 1, Seasons.Winter, 28, true, true),
		];

		foreach (var testCase in cases)
		{
			var data = new TestCaseData
			(
				testCase.supplyUpdateFrequency,
				testCase.customSupplyUpdateFrequency,
				testCase.deltaUpdateFrequency,
				testCase.customDeltaUpdateFrequency,
				testCase.year,
				testCase.season,
				testCase.dayOfMonth,
				testCase.expectedSupplyUpdate,
				testCase.expectedDeltaUpdate
			);
			
			var builder = new StringBuilder();
			builder.Append("When ");
			builder.Append("S: ");
			builder.Append(testCase.supplyUpdateFrequency == UpdateFrequency.Custom ? testCase.customSupplyUpdateFrequency : testCase.supplyUpdateFrequency);
			builder.Append(", D: ");
			builder.Append(testCase.deltaUpdateFrequency == UpdateFrequency.Custom ? testCase.customDeltaUpdateFrequency : testCase.deltaUpdateFrequency);
			builder.Append(", Y");
			builder.Append(testCase.year);
			builder.Append(" ");
			builder.Append(testCase.season);
			builder.Append(" ");
			builder.Append(testCase.dayOfMonth);
			builder.Append(" then Supply Update should be ");
			builder.Append(testCase.expectedSupplyUpdate);
			builder.Append(" and Delta Update should be ");
			builder.Append(testCase.expectedDeltaUpdate);

			data.SetName(builder.ToString());
			
			yield return data;
		}
	}
	
	[TestCaseSource(nameof(UpdateFrequencyTestCases))]
	public void ShouldDetermineUpdateFrequenciesCorrectly(
		UpdateFrequency supplyUpdateFrequency,
		int customSupplyUpdateFrequency,
		UpdateFrequency deltaUpdateFrequency,
		int customDeltaUpdateFrequency,
		int year,
		Seasons season,
		int dayOfMonth,
		bool expectedSupplyUpdate,
		bool expectedDeltaUpdate
	)
	{
		ConfigModel.Instance.SupplyUpdateFrequency = supplyUpdateFrequency;
		ConfigModel.Instance.CustomSupplyUpdateFrequency = customSupplyUpdateFrequency;
		ConfigModel.Instance.DeltaUpdateFrequency = deltaUpdateFrequency;
		ConfigModel.Instance.CustomDeltaUpdateFrequency = customDeltaUpdateFrequency;

		var result = _service.GetUpdateFrequencyInformation(new DayModel(year, season, dayOfMonth));

	Assert.Multiple(() =>
	{
	 Assert.That(result.ShouldUpdateSupply, Is.EqualTo(expectedSupplyUpdate));
	 Assert.That(result.ShouldUpdateDelta, Is.EqualTo(expectedDeltaUpdate));
	});
 }
}
