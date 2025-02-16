using System;
using System.Collections.Generic;
using fse.core.models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crops;
using Object = StardewValley.Object;

namespace fse.core.services;

public interface ISeedService
{
	void GenerateSeedMapping(EconomyModel economyModel);
	ItemModel GetItemModelFromSeedId(string seed);
	SeedModel GetSeedModelFromModelId(string modelId);
}

public class SeedService(IMonitor monitor) : ISeedService
{
	private Dictionary<string, ItemModel> SeedToItem { get; } = new();
	private Dictionary<string, SeedModel> ItemToSeed { get; } = new();
	
	public void GenerateSeedMapping(EconomyModel economyModel)
	{
		var cropData = Game1.content.Load<Dictionary<string, CropData>>("Data\\Crops");
		var failCount = 0;
		Exception mostRecentException = null;

		foreach (var seed in cropData.Keys)
		{
			try
			{
				if (seed == null)
				{
					continue;
				}

				if (!cropData.TryGetValue(seed, out var data))
				{
					continue;
				}
				var seedModel = new SeedModel(seed, data);
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
			catch (Exception ex)
			{
				failCount++;
				mostRecentException = ex;
			}
		}
		if (mostRecentException != null)
		{
			monitor.LogOnce($"Failed generating {failCount} seed mappings.", LogLevel.Error);
			monitor.LogOnce(mostRecentException.ToString(), LogLevel.Error);
		}
	}
	
	public ItemModel GetItemModelFromSeedId(string seed) => SeedToItem.GetValueOrDefault(seed);
	public SeedModel GetSeedModelFromModelId(string modelId) => ItemToSeed.GetValueOrDefault(modelId);
}