using fsd.core.actions;
using fsd.core.services;
using StardewModdingAPI;

namespace fsd.core.handlers
{
    public class SaveLoadedHandler : IHandler
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly EconomyService _economyService;

        public SaveLoadedHandler(
            IModHelper helper, 
            IMonitor monitor,
            EconomyService economyService
        )
        {
            _helper = helper;
            _monitor = monitor;
            _economyService = economyService;
        }
        
        public void Register()
        {
            _helper.Events.GameLoop.SaveLoaded += (_, _) => SafeAction.Run(GameLoopOnSaveLoaded, _monitor, nameof(GameLoopOnSaveLoaded));
        }

        private void GameLoopOnSaveLoaded()
        {
            _economyService.OnLoaded();
            /*
            */
        }
    }
}