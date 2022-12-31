using System;
using System.Linq;
using fsd.core.actions;
using fsd.core.models;
using MathNet.Numerics.Distributions;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace fsd.core.handlers
{
    public class SaveLoadedHandler : SelfRegisteringHandler
    {
        private static readonly int[] ValidCategories =
        {
            Object.GemCategory,
            Object.FishCategory,
            Object.EggCategory,
            Object.MilkCategory,
            Object.CookingCategory,
            Object.meatCategory,
            Object.metalResources,
            Object.artisanGoodsCategory,
            Object.syrupCategory,
            Object.monsterLootCategory,
            Object.VegetableCategory,
            Object.FruitsCategory,
            Object.flowersCategory,
            Object.GreensCategory
        };

        private IModHelper _helper;

        public override void Register(IModHelper helper)
        {
            _helper = helper;
            helper.Events.GameLoop.SaveLoaded += (_, _) => SafeAction.Run(GameLoopOnSaveLoaded, Monitor, nameof(GameLoopOnSaveLoaded));
        }

        private void GameLoopOnSaveLoaded()
        {
            var existingModel = _helper.Data.ReadSaveData<EconomyModel>(EconomyModel.ModelKey);
            var newModel = GenerateBlankEconomy();

            if (existingModel != null)
            {
                if (existingModel.HasSameItems(newModel))
                {
                    return;
                }
            }
            
            RandomizeEconomy(newModel, true, true);

            _helper.Data.WriteSaveData(EconomyModel.ModelKey, newModel);
        }

        private static EconomyModel GenerateBlankEconomy()
        {
            var validItems = Game1.objectInformation.Keys
                .Select(id => (id, obj: new Object(id, 1)))
                .Where(tuple => ValidCategories.Contains(tuple.obj.Category))
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
            var supplyNormal = new Normal(mean: 500, stddev: 150, randomSource: rand);
            var deltaNormal = new Normal(mean: 0, stddev: 15, randomSource: rand);

            foreach (var item in model.CategoryEconomies.Values.SelectMany(categories => categories.Values))
            {
                if (updateSupply)
                {
                    item.Supply = EnsureBounds(supplyNormal.Sample(), min: 0, max: 1000);
                }
                if (updateDelta)
                {
                    item.DailyDelta = EnsureBounds(deltaNormal.Sample(), min: -50, max: 50);
                }
            }
        }

        private static int EnsureBounds(double input, int min, int max)
        {
            var rounded = (int) Math.Round(input, 0, MidpointRounding.ToZero);

            return Math.Min(Math.Max(rounded, min), max);
        }
    }
}