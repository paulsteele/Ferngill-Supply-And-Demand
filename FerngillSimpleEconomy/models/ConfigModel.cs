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
	public float MinPercentage { get; set; } = 0.2f;
	public float MaxPercentage { get; set; } = 1.3f;
	public int MenuTabIndex { get; set; } = 10;
	public bool EnableMenuTab { get; set; } = true;
	public bool EnableShopDisplay { get; set; } = true;
}