using System.Collections.Generic;
using fse.core.models;
using StardewValley;

namespace fse.core.services;

public interface IFishService
{
	void GenerateFishMapping(EconomyModel economyModel);
	FishModel GetFishModelFromModelId(string modelId);
}

public class FishService : IFishService
{
	private Dictionary<string, FishModel> ItemToFish { get; } = new();
	
	public void GenerateFishMapping(EconomyModel economyModel)
	{
		var fishData = Game1.content.Load<Dictionary<string, string>>("Data\\Fish");

		foreach (var fish in fishData.Keys)
		{
			var fishModel = new FishModel(fish, fishData[fish]);
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
	}

	public FishModel GetFishModelFromModelId(string modelId) => ItemToFish.GetValueOrDefault(modelId);
}