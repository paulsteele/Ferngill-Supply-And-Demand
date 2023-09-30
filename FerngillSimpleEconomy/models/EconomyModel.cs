using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using StardewValley;
using Object = StardewValley.Object;

namespace fse.core.models
{
	public class EconomyModel
	{
		public static readonly string ModelKey = "fsd.economy.model";

		[JsonInclude] public Dictionary<int, Dictionary<int, ItemModel>> CategoryEconomies { get; set; }
		private Dictionary<int, ItemModel> SeedMap { get; set; } = new();

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
			Dictionary<TKey, TVal> second) => first.Count == second.Count && first.Keys.All(second.ContainsKey);

		public void ForAllItems(Action<ItemModel> action)
		{
			foreach (var item in CategoryEconomies.Values.SelectMany(categories => categories.Values))
			{
				action(item);
			}
		}

		public ItemModel GetItem(StardewValley.Object obj)
		{
			if (!CategoryEconomies.ContainsKey(obj.Category))
			{
				return null;
			}

			var category = CategoryEconomies[obj.Category];
			
			return !category.ContainsKey(obj.ParentSheetIndex) ? null : category[obj.ParentSheetIndex];
		}
		
		public void GenerateSeedMapping()
		{
			var cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");

			foreach (var seed in cropData.Keys)
			{
				var cropId = int.Parse(cropData[seed].Split('/')[3]);
				var obj = new Object(cropId, 1);
				if (!CategoryEconomies.TryGetValue(obj.Category, out var category))
				{
					continue;
				}
				if (!category.TryGetValue(cropId, out var model))
				{
					continue;
				}

				SeedMap.TryAdd(seed, model);
			}
		}

		public ItemModel GetModelFromSeedId(int seed) => SeedMap.TryGetValue(seed, out var model) ? model : default;

		public void AdvanceOneDay()
		{
			ForAllItems(model => model.AdvanceOneDay());
		}
	}
}