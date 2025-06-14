namespace fse.core.models;

public readonly record struct DayModel(int Year, Seasons Season, int DayOfMonth)
{
	public int GetTotalDayCount()
	{
		var seasonIndex = Season switch
		{
			Seasons.Spring => 0,
			Seasons.Summer => 1,
			Seasons.Fall => 2,
			Seasons.Winter => 3,
			_ => 0,
		};
		
		return (Year - 1) * (int)UpdateFrequency.Yearly + seasonIndex * (int)UpdateFrequency.Seasonally + DayOfMonth;
	}
}