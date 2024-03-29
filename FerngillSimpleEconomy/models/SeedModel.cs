using System;
using StardewValley;
using StardewValley.GameData.Crops;

namespace fse.core.models
{
	public class SeedModel
	{
		public string ObjectId { get; set; }
		public string CropId { get; set; }
		public int DaysToGrow { get; set; }
		public Seasons Seasons { get; set; }

		//1 1 1 1/spring/0/24/-1/0/false/false/false
		public SeedModel(string id, CropData cropEntry)
		{
			ObjectId = id;

			CropId = cropEntry.HarvestItemId;

			foreach (var stage in cropEntry.DaysInPhase)
			{
				DaysToGrow += stage;
			}

			var seasons = cropEntry.Seasons;

			foreach (var season in seasons)
			{
				switch (season)
				{
					case Season.Spring: Seasons |= Seasons.Spring;
						break;
					case Season.Summer: Seasons |= Seasons.Summer;
						break;
					case Season.Fall: Seasons |= Seasons.Fall;
						break;
					case Season.Winter: Seasons |= Seasons.Winter;
						break;
				}
			}
		}
	}

	[Flags]
	public enum Seasons 
	{
		Spring = 1 << 0,
		Summer = 1 << 1,
		Fall = 1 << 2,
		Winter = 1 << 3,
	}
}