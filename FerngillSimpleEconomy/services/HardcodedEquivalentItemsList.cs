namespace fse.core.services;

public class HardcodedEquivalentItemsList
{
		public static string GetEquivalentId(string id)
		{
			return id switch
			{
				//eggs
				"174" => "176",
				"180" => "176",
				"182" => "176",
				//milk
				"186" => "184",
				"438" => "436",
				_ => id
			};
		}
	
}