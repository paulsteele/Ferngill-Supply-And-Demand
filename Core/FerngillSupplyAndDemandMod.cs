using StardewModdingAPI;
using StardewValley;

namespace fsd.core
{
    public class FerngillSupplyAndDemandMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += (sender, args) =>
            {
                if (!Context.IsWorldReady)
                {
                    return;
                }

                Monitor.Log(Game1.player.Name);
            };
        }
    }
}