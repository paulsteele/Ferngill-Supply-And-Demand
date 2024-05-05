namespace fse.core.services;

public static class HardcodedArtisanItemList
{
		public static string GetArtisanBase(string id)
		{
			return id switch
			{
				// pale ale
				"303" => "304",
				// mayo
				"306" => "176",
				// duck mayo
				"307" => "442",
				// void mayo
				"308" => "305",
				// beer
				"346" => "262",
				// cheese
				"424" => "184",
				// goat cheese
				"426" => "436",
				// cloth
				"428" => "440",
				// truffle oil
				"432" => "430",
				// green tea
				"614" => "815",
				// caviar
				"445" => "698",
				// raisins
				"Raisins" => "398",
				_ => null
			};
		}
	
}