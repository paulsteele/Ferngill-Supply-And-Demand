using fse.core.models;
using StardewValley;

namespace fse.core.helpers;

public static class SeasonHelper
{
	public static Seasons GetCurrentSeason()
	{
		return Utility.getSeasonNumber(Game1.currentSeason) switch
		{
			0 => Seasons.Spring,
			1 => Seasons.Summer,
			2 => Seasons.Fall,
			3 => Seasons.Winter,
			_ => Seasons.Spring,
		};
	}
	
	public static Seasons GetNextSeason()
	{
		return Utility.getSeasonNumber(Game1.currentSeason) switch
		{
			0 => Seasons.Summer,
			1 => Seasons.Fall,
			2 => Seasons.Winter,
			3 => Seasons.Spring,
			_ => Seasons.Spring,
		};
	}
}