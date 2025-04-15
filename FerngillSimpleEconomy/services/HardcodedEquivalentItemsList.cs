namespace fse.core.services;

public static class HardcodedEquivalentItemsList
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
				"Lumisteria.MtVapius_MilkExtraLarge" => "184",
				"438" => "436",
				"Lumisteria.MtVapius_GoatMilkExtraLarge" => "436",
				//ewe Milk
				"Lumisteria.MtVapius_EweMilkLarge" => "Lumisteria.MtVapius_EweMilk",
				"Lumisteria.MtVapius_EweMilkExtraLarge" => "Lumisteria.MtVapius_EweMilk",
				//Vapius goose egg
				"Lumisteria.MtVapius_StellarGooseEgg" => "Lumisteria.MtVapius_GooseEgg",
				// SVE goose egg
				"FlashShifter.StardewValleyExpandedCP_Golden_Goose_Egg" => "FlashShifter.StardewValleyExpandedCP_Goose_Egg",
				// Phoenix
				"BrianOvaltine.Phoenixes_AshenPhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_BluePhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_GhostPhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_GoldenPhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_GreenPhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_IcePhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_IridiumPhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_MagmaPhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_PrismaticPhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_RadioactivePhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_VoidPhoenixEgg" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_AshenPhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_BluePhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_GhostPhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_GoldenPhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_GreenPhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_IcePhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_IridiumPhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_MagmaPhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_PrismaticPhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_RadioactivePhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				"BrianOvaltine.Phoenixes_VoidPhoenixFeather" => "BrianOvaltine.Phoenixes_RedPhoenixFeather",
				_ => id
			};
		}
	
}