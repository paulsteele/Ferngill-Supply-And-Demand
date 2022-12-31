using System.Collections.Generic;
using System.Linq;

namespace fsd.core.models
{
    public class EconomyModel
    {
        public static readonly string ModelKey = "fsd.economy.model";
        public Dictionary<int, Dictionary<int, ItemModel>> CategoryEconomies { get; set;}

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

        private static bool DictionariesContainSameKeys<TKey, TVal>(Dictionary<TKey, TVal> first, Dictionary<TKey, TVal> second)
        {
            return first.Count == second.Count && first.Keys.All(second.ContainsKey);
        }
    }
}