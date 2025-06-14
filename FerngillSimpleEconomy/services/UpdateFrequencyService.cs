using fse.core.helpers;
using fse.core.models;

namespace fse.core.services;

public interface IUpdateFrequencyService
{
	public UpdateFrequencyInformation GetUpdateFrequencyInformation(int year, int dayOfMonth);
}

public record UpdateFrequencyInformation(bool ShouldUpdateSupply, bool ShouldUpdateDelta, Seasons UpdateSeason);

public class UpdateFrequencyService : IUpdateFrequencyService
{
	public UpdateFrequencyInformation GetUpdateFrequencyInformation(int year, int dayOfMonth)
	{
		var totalDay = GetTotalDay(year, dayOfMonth);
		
		return new UpdateFrequencyInformation
		(
			totalDay % GetSupplyUpdateFrequencyInDays() == 0,
			totalDay % GetDeltaUpdateFrequencyInDays() == 0,
			dayOfMonth == (int)UpdateFrequency.Seasonally ? SeasonHelper.GetNextSeason() : SeasonHelper.GetCurrentSeason()
		);
	}

	private static int GetTotalDay(int year, int dayOfMonth) => (year - 1) * (int)UpdateFrequency.Yearly + dayOfMonth;

	private static int GetSupplyUpdateFrequencyInDays()
	{
		if (ConfigModel.Instance.SupplyUpdateFrequency == UpdateFrequency.Custom)
		{
			return ConfigModel.Instance.CustomSupplyUpdateFrequency;
		}

		return (int)ConfigModel.Instance.SupplyUpdateFrequency;
	}

	private static int GetDeltaUpdateFrequencyInDays()
	{
		if (ConfigModel.Instance.DeltaUpdateFrequency == UpdateFrequency.Custom)
		{
			return ConfigModel.Instance.CustomDeltaUpdateFrequency;
		}

		return (int)ConfigModel.Instance.DeltaUpdateFrequency;
	}
}