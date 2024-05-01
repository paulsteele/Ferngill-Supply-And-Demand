using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Object = StardewValley.Object;

namespace fse.core.models
{
	public class EconomyModel
	{
		public static readonly string ModelKey = "fsd.economy.model";

		[JsonInclude] public Dictionary<int, Dictionary<string, ItemModel>> CategoryEconomies { get; set; }

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