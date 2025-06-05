using System.Collections.Generic;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace fse.core.models;

public class ConfigModel
{
	public static ConfigModel Instance { get; set; } = new();
	public const int MinSupply = 0;
	public const int MaxSupply = int.MaxValue;
	public enum Frequency { FreqDay, FreqWeek, FreqSeason, FreqYear }

	public int MaxCalculatedSupply { get; set; } = 1000;
	public int MinDelta { get; set; } = -30;
	public int MaxDelta { get; set; } = 30;
	public int DeltaArrow { get; set; } = 10;
	public int StdDevSupply { get; set; } = 150;
	public int StdDevDelta { get; set; } = 5;
	public int StdDevDeltaInSeason { get; set; } = 12;
	public int StdDevDeltaOutOfSeason { get; set; } = 5;
	public float MinPercentage { get; set; } = 0.2f;
	public float MaxPercentage { get; set; } = 1.3f;
	public int MenuTabOffset { get; set; } = 0;
	public bool EnableMenuTab { get; set; } = true;
	public bool EnableShopDisplay { get; set; } = true;
	public bool EnableTooltip { get; set; } = true;
	public bool DisableArtisanMapping { get; set; } = false;
	public Frequency ChangeFreq { get; set; } = Frequency.FreqSeason; // Seasonal supply
	public Frequency SupplyFreq { get; set; } = Frequency.FreqYear; // Yearly supply
	public KeybindList ShowMenuHotkey { get; set; } = KeybindList.Parse("H");

	public List<int> ValidCategories { get; set; } =
	[
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
		-23, // Basic
		-17, // Truffles / Gem Berry
	];
}
