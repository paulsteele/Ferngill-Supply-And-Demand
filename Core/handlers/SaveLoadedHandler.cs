using System.Linq;
using fsd.core.models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

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
            helper.Events.GameLoop.SaveLoaded += GameLoopOnSaveLoaded;
        }

        private void GameLoopOnSaveLoaded(object sender, SaveLoadedEventArgs e)
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
            
            //TODO generate the actual economy

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
    }
}