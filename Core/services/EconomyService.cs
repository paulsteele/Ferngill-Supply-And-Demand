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
        private EconomyModel Economy { get; set; }

        public EconomyService(IModHelper modHelper)
        {
            _modHelper = modHelper;
        }

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
            _modHelper.Data.WriteSaveData(EconomyModel.ModelKey, newModel);
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
                CategoryEconomies = validItems
            };
        }

        private static void RandomizeEconomy(EconomyModel model, bool updateSupply, bool updateDelta)
        {
            var rand = new Random();
            var supplyNormal = new Normal(mean: ItemModel.MeanSupply, stddev: ItemModel.StdDevSupply, randomSource: rand);
            var deltaNormal = new Normal(mean: ItemModel.MeanDelta, stddev: ItemModel.StdDevDelta, randomSource: rand);

            foreach (var item in model.CategoryEconomies.Values.SelectMany(categories => categories.Values))
            {
                if (updateSupply)
                {
                    item.Supply = RoundDouble(supplyNormal.Sample());
                }
                if (updateDelta)
                {
                    item.DailyDelta = RoundDouble(deltaNormal.Sample());
                }
            }
        }

        private static int RoundDouble(double d)
        {
            return (int)Math.Round(d, 0, MidpointRounding.ToEven);
        }
    }
}