using System.Collections.Generic;
using StardewValley;

namespace fse.core.models;

public class ConfigModel
{
	public static ConfigModel Instance { get; set; }
	public const int MinSupply = 0;
	public const int MaxSupply = int.MaxValue;
	
	public int MaxCalculatedSupply { get; set; } = 1000;
	public int MinDelta { get; set; } = -30;
	public int MaxDelta { get; set; } = 30;
	public int StdDevSupply { get; set; } = 150;
	public int StdDevDelta { get; set; } = 5;
	public int StdDevDeltaInSeason { get; set; } = 15;
	public int StdDevDeltaOutOfSeason { get; set; } = 0;
	public float MinPercentage { get; set; } = 0.2f;
	public float MaxPercentage { get; set; } = 1.3f;
	public int MenuTabOffset { get; set; } = 0;
	public bool EnableMenuTab { get; set; } = true;
	public bool EnableShopDisplay { get; set; } = true;

	public List<int> ValidCategories { get; set; } = new()
	{
		Object.GemCategory,
		Object.FishCategory,
		Object.EggCategory,
		Object.MilkCategory,
		Object.meatCategory,
		Object.artisanGoodsCategory,
		Object.VegetableCategory,
		Object.FruitsCategory,
		Object.flowersCategory,
		Object.GreensCategory,
	};
}