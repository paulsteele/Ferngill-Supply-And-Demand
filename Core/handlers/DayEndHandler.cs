using fsd.core.actions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace fsd.core.handlers
{
    public class DayEndHandler : SelfRegisteringHandler
    {
        public override void Register(IModHelper helper)
        {
            helper.Events.GameLoop.DayEnding += GameLoopOnDayEnding;
        }

        private void GameLoopOnDayEnding(object sender, DayEndingEventArgs e)
        {
            SafeAction.Run(() =>
            {
                foreach (var farmer in Game1.getAllFarmers())
                {
                    foreach (Item item in Game1.getFarm().getShippingBin(farmer))
                    {
                        Monitor.Log($"sold {item.Name} at {item.salePrice()} {item.Stack}x via shipping", LogLevel.Error);
                    }
                }
            }, Monitor);
        }
    }
}