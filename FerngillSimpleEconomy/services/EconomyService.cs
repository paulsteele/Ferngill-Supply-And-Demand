using System;
using System.Collections.Generic;
using System.Linq;
using fse.core.models;
using MathNet.Numerics.Distributions;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace fse.core.services
{
	public class EconomyService
	{
		private readonly IModHelper _modHelper;
		private readonly IMonitor _monitor;
		private readonly Dictionary<int, List<int>> CategoryMapping = new();

		public bool Loaded { get; private set; }

		public EconomyService(IModHelper modHelper, IMonitor monitor)
		{
			_modHelper = modHelper;
			_monitor = monitor;
		}

		private EconomyModel Economy { get; set; }

		public void OnLoaded()
		{
			var existingModel = _modHelper.Data.ReadSaveData<EconomyModel>(EconomyModel.ModelKey);
			var newModel = GenerateBlankEconomy();

			if (existingModel != null && existingModel.HasSameItems(newModel))
			{
				Economy = existingModel;
				Economy.GenerateSeedMapping();
				Loaded = true;
				return;
			}

			RandomizeEconomy(newModel, true, true);

			Economy = newModel;
			ConsolidateEconomyCategories();
			Economy.GenerateSeedMapping();
			
			QueueSave();
			Loaded = true;
		}

		private void ConsolidateEconomyCategories()
		{
			foreach (var matchingCategories in GetCategories()
				.GroupBy(pair => pair.Value)
				.Where(pairs => pairs.Count() > 1)
			)
			{
				var categories = matchingCategories.ToArray().Select(pair => pair.Key).ToArray();
				var key = categories[0];
				var remainingCategories = categories.Skip(1).ToList();
				
				CategoryMapping.TryAdd(key, remainingCategories);
			}
		}

		private void QueueSave()
		{
			_modHelper.Data.WriteSaveData(EconomyModel.ModelKey, Economy);
		}


		private static EconomyModel GenerateBlankEconomy()
		{
			var validItems = Game1.objectInformation.Keys
				.Where(key => !EconomyIgnoreList.IgnoreList.Contains(key))
				.Select(id => new Object(id, 1))
				.Where(obj => EconomyValidCategories.Categories.Contains(obj.Category))
				.GroupBy(obj => obj.Category, obj => new ItemModel { ObjectId = obj.ParentSheetIndex })
				.ToDictionary(grouping => grouping.Key, grouping => grouping.ToDictionary(item => item.ObjectId));

			return new EconomyModel
			{
				CategoryEconomies = validItems,
			};
		}

		public void SetupForNewSeason()
		{
			RandomizeEconomy(Economy, false, true);
			Economy.ForAllItems(model => model.CapSupply());
			QueueSave();
		}

		private static void RandomizeEconomy(EconomyModel model, bool updateSupply, bool updateDelta)
		{
			var rand = new Random();
			var supplyNormal = new Normal(ItemModel.MeanSupply, ItemModel.StdDevSupply, rand);
			var deltaNormal = new Normal(ItemModel.MeanDelta, ItemModel.StdDevDelta, rand);

			model.ForAllItems(item =>
			{
				if (updateSupply)
				{
					item.Supply = RoundDouble(supplyNormal.Sample());
				}

				if (updateDelta)
				{
					item.DailyDelta = RoundDouble(deltaNormal.Sample());
				}
			});
		}

		public void AdvanceOneDay()
		{
			if (Economy == null)
			{
				return;
			}

			Economy.AdvanceOneDay();
			QueueSave();
		}

		public Dictionary<int, string> GetCategories()
		{
			return Economy.CategoryEconomies
				.Where(pair => pair.Value.Values.Count > 0)
				.ToDictionary(pair => pair.Key, pair => new Object(pair.Value.Values.First().ObjectId, 1).getCategoryName());
		}

		public ItemModel[] GetItemsForCategory(int category)
		{
			var items = Economy.CategoryEconomies.Keys.Contains(category)
				? Economy.CategoryEconomies[category].Values.ToArray()
				: Array.Empty<ItemModel>();

			return 
				!CategoryMapping.ContainsKey(category) 
					? items 
					: CategoryMapping[category]
						.Aggregate(items, (current, adjacentCategory) => current.Concat(Economy.CategoryEconomies[adjacentCategory].Values).ToArray());
		}

		public int GetPrice(Object obj, int basePrice)
		{
			if (Economy == null)
			{
				_monitor.Log($"Economy not generated to determine item model for {obj.name}", LogLevel.Error);
				return basePrice;
			}
			var itemModel = Economy.GetItem(obj);
			if (itemModel == null)
			{
				_monitor.Log($"Could not find item model for {obj.name}", LogLevel.Trace);
				return basePrice;
			}
			var adjustedPrice = itemModel.GetPrice(basePrice);
			
			_monitor.Log($"Altered {obj.name} from {basePrice} to {adjustedPrice}", LogLevel.Trace);

			return adjustedPrice;
		}

		public void AdjustSupply(Object obj, int amount)
		{
			if (Economy == null)
			{
				_monitor.Log($"Economy not generated to determine item model for {obj.name}", LogLevel.Error);
				return;
			}
			var itemModel = Economy.GetItem(obj);
			if (itemModel == null)
			{
				_monitor.Log($"Could not find item model for {obj.name}", LogLevel.Trace);
				return;
			}

			var prev = itemModel.Supply;
			itemModel.Supply += amount;

			_monitor.Log($"Adjusted {obj.name} supply from {prev} to {itemModel.Supply}", LogLevel.Trace);
			QueueSave();
		}

		public ItemModel GetItemModelFromSeed(int seed) => Economy.GetItemModelFromSeedId(seed);
		private SeedModel GetSeedModelFromItem(int item) => Economy.GetSeedModelFromModelId(item);

		public bool ItemValidForSeason(ItemModel model, Seasons seasonsFilter)
		{
			var seed = GetSeedModelFromItem(model.ObjectId);

			if (seed == null)
			{
				return true;
			}

			return (seed.Seasons & seasonsFilter) != 0;
		}

		public int GetPricePerDay(ItemModel model)
		{
			var seed = GetSeedModelFromItem(model.ObjectId);

			var modelPrice = model.GetPrice(model.GetObjectInstance().Price);
			
			if (seed == null || seed.DaysToGrow < 1)
			{
				return modelPrice;
			}

			return modelPrice / seed.DaysToGrow;
		}

		private static int RoundDouble(double d) => (int)Math.Round(d, 0, MidpointRounding.ToEven);
	}
}