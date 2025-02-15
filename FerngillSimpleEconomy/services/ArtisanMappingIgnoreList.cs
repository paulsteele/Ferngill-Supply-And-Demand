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
	];
}