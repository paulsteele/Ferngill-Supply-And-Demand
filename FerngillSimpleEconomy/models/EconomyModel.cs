using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Crops;
using Object = StardewValley.Object;

namespace fse.core.models
{
	public class EconomyModel
	{
		public static readonly string ModelKey = "fsd.economy.model";

		[JsonInclude] public Dictionary<int, Dictionary<string, ItemModel>> CategoryEconomies { get; set; }
		private Dictionary<string, ItemModel> SeedToItem { get; } = new();
		private Dictionary<string, SeedModel> ItemToSeed { get; } = new();
		private Dictionary<string, FishModel> ItemToFish { get; } = new();

		public bool HasSameItems(EconomyModel other)
		{
			if (!DictionariesContainSameKeys(CategoryEconomies, other.CategoryEconomies))
			{
				return false;
			}

			return !(
					from key in CategoryEconomies.Keys
					let category = CategoryEconomies[key]
					let otherCategory = other.CategoryEconomies[key]
					where !DictionariesContainSameKeys(category, otherCategory)
					select category)
				.Any();
		}

		private static bool DictionariesContainSameKeys<TKey, TVal>(Dictionary<TKey, TVal> first,
			IReadOnlyDictionary<TKey, TVal> second) => first.Count == second.Count && first.Keys.All(second.ContainsKey);

		public void ForAllItems(Action<ItemModel> action)
		{
			foreach (var item in CategoryEconomies.Values.SelectMany(categories => categories.Values))
			{
				action(item);
			}
		}

		public ItemModel GetItem(Object obj) => 
			CategoryEconomies.TryGetValue(obj.Category, out var category) 
				? category.GetValueOrDefault(obj.ItemId) 
				: null;

		public void GenerateSeedMapping()
		{
			var cropData = Game1.content.Load<Dictionary<string, CropData>>("Data\\Crops");

			foreach (var seed in cropData.Keys)
			{
				var seedModel = new SeedModel(seed, cropData[seed]);
				var obj = new Object(seedModel.CropId, 1);
				if (!CategoryEconomies.TryGetValue(obj.Category, out var category))
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

		public void GenerateFishMapping()
		{
			var fishData = Game1.content.Load<Dictionary<string, string>>("Data\\Fish");

			foreach (var fish in fishData.Keys)
			{
				var fishModel = new FishModel(fish, fishData[fish]);
				var obj = new Object(fishModel.ObjectId, 1);
				if (!CategoryEconomies.TryGetValue(obj.Category, out var category))
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

		public ItemModel GetItemModelFromSeedId(string seed) => SeedToItem.GetValueOrDefault(seed);
		public SeedModel GetSeedModelFromModelId(string modelId) => ItemToSeed.GetValueOrDefault(modelId);
		public FishModel GetFishModelFromModelId(string modelId) => ItemToFish.GetValueOrDefault(modelId);

		public void AdvanceOneDay()
		{
			ForAllItems(model => model.AdvanceOneDay());
		}

		public void UpdateAllMultipliers()
		{
			ForAllItems(model => model.UpdateMultiplier());
		}
	}
}