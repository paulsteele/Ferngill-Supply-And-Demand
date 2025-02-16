namespace fse.core.services;

//necessary to avoid certain items being the unexpected base of artisan goods
public static class ArtisanMappingIgnoreList
{
	public static readonly string[] IgnoreList =
	[
		"289", // Ostrich Egg (Mayo)
		"928", // Golden Egg (Mayo)
		"388", // Wood (coal)
		"709", // Hardwood (syrup)
		"910", // Radioactive bar (slot machine)
		"337", // Iridium bar (slot machine)
		"336", // Gold bar (slot machine)
		"335", // Iron bar (slot machine)
		"334", // Copper bar (slot machine)
	];
}