using System;
using System.Linq;
using fsd.core.models;
using MathNet.Numerics.Distributions;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace fsd.core.services
{
	public class EconomyService
	{
		private readonly IModHelper _modHelper;

		public EconomyService(IModHelper modHelper) => _modHelper = modHelper;

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
				.Select(id => (id, obj: new Object(id, 1)))
				.Where(tuple => EconomyValidCategories.Categories.Contains(tuple.obj.Category))
				.GroupBy(tuple => tuple.obj.Category, tuple => new ItemModel { ObjectId = tuple.id })
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

		private static int RoundDouble(double d) => (int)Math.Round(d, 0, MidpointRounding.ToEven);
	}
}