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
				//blue egg
				"6480.blueegg_BlueMayo" => "6480.blueegg_BlueEgg",
				"6480.blueegg_GoldenMayo" => "928",
				"6480.blueegg_OstrichMayo" => "289",
				// Vapius Ewe
				"Lumisteria.MtVapius_EweCheese" => "Lumisteria.MtVapius_EweMilk",
				// Vapius Goose
				"Lumisteria.MtVapius_GooseMayonnaise" => "Lumisteria.MtVapius_GooseEgg",
				"Lumisteria.MtVapius_StellarGooseMayonnaise" => "Lumisteria.MtVapius_StellarGooseEgg",
				// SVE Goose
				"FlashShifter.StardewValleyExpandedCP_Goose_Mayonnaise" => "FlashShifter.StardewValleyExpandedCP_Goose_Egg",
				// Vapius speckled
				"Lumisteria.MtVapius_SpeckledFowlMayonnaise" => "Lumisteria.MtVapius_SpeckledFowlEgg",
				"Lumisteria.MtVapius_SpeckledFowlVoidMayonnaise" => "Lumisteria.MtVapius_SpeckledFowlVoidEgg",
				// Phoenix
				"BrianOvaltine.Phoenixes_VoidPhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_AshenPhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_BluePhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_GhostPhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_GoldenPhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_GreenPhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_IcePhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_IridiumPhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_MagmaPhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_PrismaticPhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_RadioactivePhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				"BrianOvaltine.Phoenixes_RedPhoenixMayonnaise" => "BrianOvaltine.Phoenixes_RedPhoenixEgg",
				_ => null
			};
		}
	
}