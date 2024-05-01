using System.Collections.Generic;
using fse.core.models;
using StardewValley;
using StardewValley.GameData.Crops;

namespace fse.core.services;

public interface ISeedService
{
	void GenerateSeedMapping(EconomyModel economyModel);
	ItemModel GetItemModelFromSeedId(string seed);
	SeedModel GetSeedModelFromModelId(string modelId);
}

public class SeedService : ISeedService
{
	private Dictionary<string, ItemModel> SeedToItem { get; } = new();
	private Dictionary<string, SeedModel> ItemToSeed { get; } = new();
	
	public void GenerateSeedMapping(EconomyModel economyModel)
	{
		var cropData = Game1.content.Load<Dictionary<string, CropData>>("Data\\Crops");

		foreach (var seed in cropData.Keys)
		{
			var seedModel = new SeedModel(seed, cropData[seed]);
			var obj = new Object(seedModel.CropId, 1);
			if (!economyModel.CategoryEconomies.TryGetValue(obj.Category, out var category))
			{
				continue;
			}
			if (!category.TryGetValue(seedModel.CropId, out var model))
			{
				continue;
			}

			SeedToItem.TryAdd(seedModel.ObjectId, model);
			ItemToSeed.TryAdd(model.ObjectId, seedModel);
		}
	}
	
	public ItemModel GetItemModelFromSeedId(string seed) => SeedToItem.GetValueOrDefault(seed);
	public SeedModel GetSeedModelFromModelId(string modelId) => ItemToSeed.GetValueOrDefault(modelId);
}