using System.Linq;
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
        
        public override void Register(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += GameLoopOnSaveLoaded;
        }

        private void GameLoopOnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var validItems = Game1.objectInformation.Keys
                .Select(id => new Object(id, 1))
                .Where(obj => ValidCategories.Contains(obj.Category));
        }
    }
}