using fse.core.helpers;
using fse.core.models;

namespace fse.core.services;

public interface IUpdateFrequencyService
{
	public UpdateFrequencyModel GetUpdateFrequencyInformation(DayModel dayModel);
}

public class UpdateFrequencyService : IUpdateFrequencyService
{
	public UpdateFrequencyModel GetUpdateFrequencyInformation(DayModel dayModel)
	{
		var totalDays = dayModel.GetTotalDayCount();
		
		return new UpdateFrequencyModel
		(
			totalDays % GetSupplyUpdateFrequencyInDays() == 0,
			totalDays % GetDeltaUpdateFrequencyInDays() == 0,
			dayModel.DayOfMonth == (int)UpdateFrequency.Seasonally ? SeasonHelper.GetNextSeason() : SeasonHelper.GetCurrentSeason()
		);
	}

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