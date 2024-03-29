namespace fse.core.models
{
	public class FishModel
	{
		public string ObjectId { get; set; }
		public Seasons Seasons { get; set; }

		public FishModel(string id, string fishEntry)
		{
			ObjectId = id;

			var data = fishEntry.Split('/');
			
			var seasons = data[6].Split(" ");

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
}