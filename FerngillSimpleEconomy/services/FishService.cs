using System;
using System.Collections.Generic;
using fse.core.models;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace fse.core.services;

public interface IFishService
{
	void GenerateFishMapping(EconomyModel economyModel);
	FishModel GetFishModelFromModelId(string modelId);
}

public class FishService(IMonitor monitor) : IFishService
{
	private Dictionary<string, FishModel> ItemToFish { get; } = new();
	
	public void GenerateFishMapping(EconomyModel economyModel)
	{
		var fishData = Game1.content.Load<Dictionary<string, string>>("Data\\Fish");
		var failCount = 0;
		Exception mostRecentException = null;

		foreach (var fish in fishData.Keys)
		{
			try
			{
				if (!fishData.TryGetValue(fish, out var data))
				{
					continue;
				}
				var fishModel = new FishModel(fish, data);
				var obj = new Object(fishModel.ObjectId, 1);
				if (!economyModel.CategoryEconomies.TryGetValue(obj.Category, out var category))
				{
					continue;
				}

				if (!category.TryGetValue(fishModel.ObjectId, out var itemModel))
				{
					continue;
				}

				ItemToFish.TryAdd(itemModel.ObjectId, fishModel);
			}
			catch (Exception ex)
			{
				failCount++;
				mostRecentException = ex;
			}
		}

		if (mostRecentException != null)
		{
			monitor.Log($"Failed generating {failCount} fish mappings.", LogLevel.Error);
			monitor.Log(mostRecentException.Message, LogLevel.Error);
		}
	}

	public FishModel GetFishModelFromModelId(string modelId) => ItemToFish.GetValueOrDefault(modelId);
}