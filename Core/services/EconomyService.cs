﻿using System;
using System.Linq;
using System.Threading;
using fsd.core.models;
using MathNet.Numerics.Distributions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using Object = StardewValley.Object;

namespace fsd.core.services
{
	public class EconomyService
	{
		private readonly IModHelper _modHelper;
		private readonly IMonitor _monitor;

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
				return;
			}

			RandomizeEconomy(newModel, true, true);

			Economy = newModel;
			QueueSave();
		}

		private void QueueSave()
		{
			_modHelper.Data.WriteSaveData(EconomyModel.ModelKey, Economy);
		}

		private static EconomyModel GenerateBlankEconomy()
		{
			var validItems = Game1.objectInformation.Keys
				.Select(id => new Object(id, 1))
				.Where(obj => EconomyValidCategories.Categories.Contains(obj.Category))
				.GroupBy(obj => obj.Category, obj => new ItemModel { ObjectId = obj.ParentSheetIndex })
				.ToDictionary(grouping => grouping.Key, grouping => grouping.ToDictionary(item => item.ObjectId));

			return new EconomyModel
			{
				CategoryEconomies = validItems,
			};
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
				_monitor.Log($"Could not find item model for {obj.name}", LogLevel.Error);
				return basePrice;
			}
			var adjustedPrice = itemModel.GetPrice(basePrice);
			
			_monitor.Log($"Altered {obj.name} from {basePrice} to {adjustedPrice}", LogLevel.Error);

			return adjustedPrice;
		}

		private static int RoundDouble(double d) => (int)Math.Round(d, 0, MidpointRounding.ToEven);
	}
}