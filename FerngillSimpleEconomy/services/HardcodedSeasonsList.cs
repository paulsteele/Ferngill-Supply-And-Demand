using fse.core.models;

namespace fse.core.services
{
	public static class HardcodedSeasonsList
	{
		public static Seasons GetSeasonForItem(int id)
		{
			return id switch
			{
				16 => Seasons.Spring,
				18 => Seasons.Spring,
				20 => Seasons.Spring,
				22 => Seasons.Spring,
				257 => Seasons.Spring,
				281 => Seasons.Fall,
				283 => Seasons.Winter,
				296 => Seasons.Spring,
				396 => Seasons.Summer,
				398 => Seasons.Summer,
				399 => Seasons.Spring,
				402 => Seasons.Summer,
				404 => Seasons.Spring | Seasons.Summer | Seasons.Fall,
				406 => Seasons.Fall,
				408 => Seasons.Fall,
				410 => Seasons.Fall,
				412 => Seasons.Winter,
				414 => Seasons.Winter,
				416 => Seasons.Winter,
				418 => Seasons.Winter,
				420 => Seasons.Summer | Seasons.Fall,
				422 => Seasons.Fall,
				_ => Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter,
			};
		}
	}
}