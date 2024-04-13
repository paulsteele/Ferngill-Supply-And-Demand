using System;
using System.Collections.Generic;
using System.Linq;
using fse.core.extensions;
using fse.core.models;
using fse.core.multiplayer;
using MathNet.Numerics.Distributions;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace fse.core.services
{
	public class EconomyService(IModHelper modHelper, IMonitor monitor, IMultiplayerService multiplayerService)
	{
		private readonly Dictionary<int, List<int>> CategoryMapping = new();

		public bool Loaded { get; private set; }

		private EconomyModel Economy { get; set; }

		public void OnLoaded()
		{
			if (IsClient)
			{
				multiplayerService.SendMessageToPeers(new RequestEconomyModelMessage());
				return;
			}
			var existingModel = modHelper.Data.ReadSaveData<EconomyModel>(EconomyModel.ModelKey);
			var newModel = GenerateBlankEconomy();
			var needToSave = false;

			if (existingModel != null && existingModel.HasSameItems(newModel))
			{
				Economy = existingModel;
			}
			else
			{
				RandomizeEconomy(newModel, true, true);
				Economy = newModel;
				needToSave = true;
			}
	
			EconomySetup();

			if (needToSave)
			{
				QueueSave();
			}
			Loaded = true;
		}

		public void ReceiveEconomy(EconomyModel economyModel)
		{
			Economy = economyModel;
			EconomySetup();
			Loaded = true;
		}

		private void EconomySetup()
		{
			ConsolidateEconomyCategories();
			Economy.GenerateSeedMapping();
			Economy.GenerateFishMapping();
			Economy.UpdateAllMultipliers();
		}

		public void SendEconomyMessage() => multiplayerService.SendMessageToPeers(new EconomyModelMessage(Economy));

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
			if (IsClient)
			{
				return;
			}
			modHelper.Data.WriteSaveData(EconomyModel.ModelKey, Economy);
		}

		private static EconomyModel GenerateBlankEconomy()
		{
			var validItems = Game1.objectData.Keys
				.Where(key => !EconomyIgnoreList.IgnoreList.Contains(key))
				.Select(id => new Object(id, 1))
				.Where(obj => ConfigModel.Instance.ValidCategories.Contains(obj.Category))
				.GroupBy(obj => obj.Category, obj => new ItemModel { ObjectId = obj.ItemId })
				.ToDictionary(grouping => grouping.Key, grouping => grouping.ToDictionary(item => item.ObjectId));

			return new EconomyModel
			{
				CategoryEconomies = validItems,
			};
		}

		public void SetupForNewSeason()
		{
			if (IsClient)
			{
				return;
			}
			RandomizeEconomy(Economy, false, true);
			Economy.ForAllItems(model => model.CapSupply());
			QueueSave();
		}
		
		public void SetupForNewYear()
		{
			if (IsClient)
			{
				return;
			}
			RandomizeEconomy(Economy, true, true);
			QueueSave();
		}
		
		public static int MeanSupply => (ConfigModel.MinSupply + ConfigModel.Instance.MaxCalculatedSupply) / 2;
		public static int MeanDelta => (ConfigModel.Instance.MinDelta + ConfigModel.Instance.MaxDelta) / 2;

		private static void RandomizeEconomy(EconomyModel model, bool updateSupply, bool updateDelta)
		{
			var rand = new Random();
			var supplyNormal = new Normal(MeanSupply, ConfigModel.Instance.StdDevSupply, rand);
			var deltaNormal = new Normal(MeanDelta, ConfigModel.Instance.StdDevDelta, rand);

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
			if (IsClient)
			{
				return;
			}
			if (Economy == null)
			{
				return;
			}

			Economy.AdvanceOneDay();
			SendEconomyMessage();
			QueueSave();
		}

		private static bool IsClient => !Game1.player.IsMainPlayer;

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
				monitor.Log($"Economy not generated to determine item model for {obj.name}", LogLevel.Error);
				return basePrice;
			}

			if (obj.Category == Object.artisanGoodsCategory)
			{
				var price = GetArtisanGoodPrice(obj, basePrice);
				if (price > 0)
				{
					return price;
				}
			}
			
			var itemModel = Economy.GetItem(obj);
			if (itemModel == null)
			{
				// monitor.Log($"Could not find item model for {obj.name}", LogLevel.Trace);
				return basePrice;
			}
			var adjustedPrice = itemModel.GetPrice(basePrice);
			
			// monitor.Log($"Altered {obj.name} from {basePrice} to {adjustedPrice}", LogLevel.Trace);

			return adjustedPrice;
		}

		private Object GetArtisanBase(Object obj)
		{
			var preserveId = obj.preservedParentSheetIndex?.Get();
			return string.IsNullOrWhiteSpace(preserveId)  ? null : new Object(preserveId, 1);
		}

		private int GetArtisanGoodPrice(Object obj, int price)
		{
			var artisanBase = GetArtisanBase(obj);

			if (artisanBase == null)
			{
				return -1;
			}

			var basePrice = GetPrice(artisanBase, artisanBase.Price);

			if (artisanBase.Price < 1)
			{
				return -1;
			}
			
			var modifier = price / (double)artisanBase.Price;

			return (int)(basePrice * modifier);
		}

		public void AdjustSupply(Object obj, int amount, bool notifyPeers = true)
		{
			if (Economy == null)
			{
				monitor.Log($"Economy not generated to determine item model for {obj.name}", LogLevel.Error);
				return;
			}

			if (obj.Category == Object.artisanGoodsCategory)
			{
				obj = GetArtisanBase(obj) ?? obj;
			}
			
			var itemModel = Economy.GetItem(obj);
			if (itemModel == null)
			{
				// monitor.Log($"Could not find item model for {obj.name}", LogLevel.Trace);
				return;
			}

			var prev = itemModel.Supply;
			itemModel.Supply += amount;

			// monitor.Log($"Adjusted {obj.name} supply from {prev} to {itemModel.Supply}", LogLevel.Trace);

			if (notifyPeers)
			{
				multiplayerService.SendMessageToPeers(new SupplyAdjustedMessage(itemModel.ObjectId, amount));
			}
			
			if (!IsClient)
			{
				QueueSave();
			}
		}

		public ItemModel GetItemModelFromSeed(string seed) => Economy.GetItemModelFromSeedId(seed);
		private SeedModel GetSeedModelFromItem(string item) => Economy.GetSeedModelFromModelId(item);
		private FishModel GetFishModelFromItem(string item) => Economy.GetFishModelFromModelId(item);

		public bool ItemValidForSeason(ItemModel model, Seasons seasonsFilter)
		{
			var seed = GetSeedModelFromItem(model.ObjectId);

			if (seed != null)
			{
				return (seed.Seasons & seasonsFilter) != 0;
			}

			var fish = GetFishModelFromItem(model.ObjectId);
			if (fish != null)
			{
				return (fish.Seasons & seasonsFilter) != 0;
			}

			return (HardcodedSeasonsList.GetSeasonForItem(model.ObjectId) & seasonsFilter) != 0;
		}

		public int GetPricePerDay(ItemModel model)
		{
			var seed = GetSeedModelFromItem(model.ObjectId);

			var modelPrice = model.GetObjectInstance().sellToStorePrice();
			
			if (seed == null || seed.DaysToGrow < 1)
			{
				return -1;
			}

			return modelPrice / seed.DaysToGrow;
		}

		private static int RoundDouble(double d) => (int)Math.Round(d, 0, MidpointRounding.ToEven);
	}
}