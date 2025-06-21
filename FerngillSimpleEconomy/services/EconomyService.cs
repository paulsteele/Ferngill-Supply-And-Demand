using System;
using System.Collections.Generic;
using System.Linq;
using fse.core.helpers;
using fse.core.models;
using fse.core.multiplayer;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace fse.core.services;

public interface IEconomyService
{
	bool Loaded { get; }
	void OnLoaded();
	void ReceiveEconomy(EconomyModel economyModel);
	void SendEconomyMessage();
	void Reset(bool updateSupply, bool updateDelta, Seasons season);
	void HandleDayEnd(DayModel dayModel);
	void AdvanceOneDay();
	Dictionary<int, string> GetCategories();
	ItemModel[] GetItemsForCategory(int category);
	int GetPrice(Object obj, int basePrice);
	void AdjustSupply(Object? obj, int amount, bool notifyPeers = true);
	ItemModel? GetItemModelFromSeed(string seed);
	bool ItemValidForSeason(ItemModel model, Seasons seasonsFilter);
	int GetPricePerDay(ItemModel model);
	ItemModel GetConsolidatedItem(ItemModel original);
	float GetBreakEvenSupply();
	ItemModel? GetItemModelFromObject(Object obj);
}

public class EconomyService(
	IModHelper modHelper, 
	IMonitor monitor, 
	IMultiplayerService multiplayerService,
	IFishService fishService,
	ISeedService seedService,
	IArtisanService artisanService,
	INormalDistributionService normalDistributionService,
	IUpdateFrequencyService updateFrequencyService
) : IEconomyService
{
	private readonly Dictionary<int, List<int>> _categoryMapping = new();

	public bool Loaded { get; private set; }

	private EconomyModel Economy { get; set; } = new(new Dictionary<int, Dictionary<string, ItemModel>>());

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

		if (existingModel != null)
		{
			if (existingModel.HasSameItems(newModel))
			{
				Economy = existingModel;
			}
			else
			{
				RandomizeEconomy(newModel, true, true, SeasonHelper.GetCurrentSeason());
				
				newModel.ForAllItems(i =>
				{
					var old = existingModel.GetItem(i.ObjectId);
					if (old == null)
					{
						return;
					}

					i.Supply = old.Supply;
					i.DailyDelta = old.DailyDelta;
				});
				
				Economy = newModel;
				needToSave = true;
			}
		}
		else
		{
			RandomizeEconomy(newModel, true, true, SeasonHelper.GetCurrentSeason());
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
		seedService.GenerateSeedMapping(Economy);
		fishService.GenerateFishMapping(Economy);
		artisanService.GenerateArtisanMapping(Economy);
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
				
			_categoryMapping.TryAdd(key, remainingCategories);
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
			.GroupBy(obj => obj.Category, obj => new ItemModel(obj.ItemId))
			.ToDictionary(grouping => grouping.Key, grouping => grouping.ToDictionary(item => item.ObjectId));

		return new EconomyModel(validItems);
	}

	public void Reset(bool updateSupply, bool updateDelta, Seasons season)
	{
		if (IsClient)
		{
			return;
		}
		RandomizeEconomy(Economy, updateSupply, updateDelta, season);
		Economy.ForAllItems(model => model.CapSupply());
		QueueSave();
	}

	public void HandleDayEnd(DayModel dayModel)
	{
		var (shouldUpdateSupply, shouldUpdateDelta, season) = updateFrequencyService.GetUpdateFrequencyInformation(dayModel);

		if (!shouldUpdateSupply && !shouldUpdateDelta)
		{
			return;
		}

		Reset(shouldUpdateSupply, shouldUpdateDelta, season);
	}

	private void RandomizeEconomy(EconomyModel model, bool updateSupply, bool updateDelta, Seasons season)
	{
		normalDistributionService.Reset();

		model.ForAllItems(item =>
		{
			if (updateSupply)
			{
				item.Supply = RoundDouble(normalDistributionService.SampleSupply());
			}

			if (!updateDelta)
			{
				return;
			}

			if (ItemIsSeasonal(item))
			{
				item.DailyDelta = ItemValidForSeason(item, season)
					? RoundDouble(normalDistributionService.SampleInSeasonDelta())
					: RoundDouble(normalDistributionService.SampleOutOfSeasonDelta());
			}
			else
			{
				item.DailyDelta = RoundDouble(normalDistributionService.SampleSeasonlessDelta());
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
		var items = Economy.CategoryEconomies.TryGetValue(category, out var economy) ? economy.Values.ToArray() : [];

		return 
			!_categoryMapping.TryGetValue(category, out var value) 
				? items 
				: value
					.Aggregate(items, (current, adjacentCategory) => current.Concat(Economy.CategoryEconomies[adjacentCategory].Values).ToArray());
	}

	public int GetPrice(Object obj, int basePrice)
	{
		if (Economy == null)
		{
			monitor.LogOnce($"Economy not generated to determine item model for {obj.name}", LogLevel.Trace);
			return basePrice;
		}

		var artisanBase = GetArtisanBase(obj);

		if (artisanBase != null)
		{
			var price = GetArtisanGoodPrice(artisanBase, basePrice);
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

	public float GetBreakEvenSupply()
	{
		if (ConfigModel.Instance.MaxPercentage < 1f)
		{
			return -1;
		}

		if (ConfigModel.Instance.MinPercentage > 1f)
		{
			return -1;
		}
		
		var den = ConfigModel.Instance.MaxPercentage - ConfigModel.Instance.MinPercentage;

		if (den == 0)
		{
			return -1;
		}

		var num = (ConfigModel.Instance.MinPercentage * ConfigModel.Instance.MaxCalculatedSupply) - ConfigModel.Instance.MaxCalculatedSupply;

		return (num / den) + ConfigModel.Instance.MaxCalculatedSupply;
	}

	private Object? GetArtisanBase(Object? obj)
	{
		if (obj == null)
		{
			return null;
		}
		var preserveId = obj.preservedParentSheetIndex?.Get();
		var artisanBase = artisanService.GetBaseFromArtisanGood(obj.ItemId);
		if (artisanBase != null)
		{
			return artisanBase.GetObjectInstance();
		}
		return string.IsNullOrWhiteSpace(preserveId) ? null : new Object(preserveId, 1);
	}

	private int GetArtisanGoodPrice(Object artisanBase, int price)
	{
		var basePrice = GetPrice(artisanBase, artisanBase.Price);

		if (artisanBase.Price < 1)
		{
			return -1;
		}
			
		var modifier = price / (double)artisanBase.Price;

		return (int)(basePrice * modifier);
	}

	public void AdjustSupply(Object? obj, int amount, bool notifyPeers = true)
	{
		obj = GetArtisanBase(obj) ?? obj;
			
		var itemModel = Economy.GetItem(obj);
		if (itemModel == null)
		{
			// monitor.Log($"Could not find item model for {obj.name}", LogLevel.Trace);
			return;
		}

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

	public ItemModel? GetItemModelFromSeed(string seed) => seedService.GetItemModelFromSeedId(seed);
	private SeedModel? GetSeedModelFromItem(string item) => seedService.GetSeedModelFromModelId(item);
	private FishModel? GetFishModelFromItem(string item) => fishService.GetFishModelFromModelId(item);

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

	private bool ItemIsSeasonal(ItemModel model)
	{
		var seed = GetSeedModelFromItem(model.ObjectId);

		if (seed != null && seed.Seasons != 0)
		{
			return true;
		}
		
		var fish = GetFishModelFromItem(model.ObjectId);

		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (fish != null && fish.Seasons != 0)
		{
			return true;
		}

		return HardcodedSeasonsList.GetSeasonForItem(model.ObjectId) != (Seasons.Spring | Seasons.Summer | Seasons.Fall | Seasons.Winter);
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

	public ItemModel GetConsolidatedItem(ItemModel original)
	{
		var baseModel = Economy.GetItem(original.ObjectId);
		if (baseModel == null)
		{
			return original;
		}

		var artisanBase = artisanService.GetBaseFromArtisanGood(baseModel.ObjectId);

		return artisanBase ?? baseModel;
	}

	public ItemModel? GetItemModelFromObject(Object obj)
	{
		var artisanBase = GetArtisanBase(obj);
		if (artisanBase != null)
		{
			obj = artisanBase;
		}
		
		var model = Economy.GetItem(obj);
		return model == null ? null : GetConsolidatedItem(model);
	}

	private static int RoundDouble(double d) => (int)Math.Round(d, 0, MidpointRounding.ToEven);
}
