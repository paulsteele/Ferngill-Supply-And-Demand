using System;

namespace fse.core.models
{
	public class SeedModel
	{
		public int ObjectId { get; set; }
		public int CropId { get; set; }
		public int DaysToGrow { get; set; }
		public Seasons Seasons { get; set; }

		//1 1 1 1/spring/0/24/-1/0/false/false/false
		public SeedModel(int id, string cropEntry)
		{
			ObjectId = id;
			var data = cropEntry.Split('/');

			if (int.TryParse(data[3], out var itemId))
			{
				CropId = itemId;
			}

			var growth = data[0].Split(" ");
			foreach (var stage in growth)
			{
				if (int.TryParse(stage, out var stageLength))
				{
					DaysToGrow += stageLength;
				}
			}

			var seasons = data[1].Split(" ");

			foreach (var season in seasons)
			{
				switch (season)
				{
					case "spring": Seasons |= Seasons.Spring;
						break;
					case "summer": Seasons |= Seasons.Summer;
						break;
					case "fall": Seasons |= Seasons.Fall;
						break;
					case "winter": Seasons |= Seasons.Winter;
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