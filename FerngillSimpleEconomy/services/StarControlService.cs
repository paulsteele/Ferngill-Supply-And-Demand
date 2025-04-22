using System;
using fse.core.integrations;
using StardewModdingAPI;
using StardewValley;

namespace fse.core.services;

public interface IStarControlService
{
    void Register(IStarControlApi? api);
}

public class StarControlService(
    IModHelper helper, 
    IForecastMenuService forecastMenuService,
    IIconicFrameworkApi? iconicFrameworkApi) : IStarControlService
{
    public void Register(IStarControlApi? api)
    {
        if (api == null)
        {
            return;
        }
        
        // Early return if IconicFrameworkApi is available through DI
        if (iconicFrameworkApi != null)
        {
            return;
        }
        
        Console.WriteLine(helper.ToString());
        Console.WriteLine(forecastMenuService.ToString());

        // TODO: Implement the registration logic for StarControl integration
        // This should follow a similar pattern to IconicFrameworkService but adapted for StarControl's API
        
        // Example implementation (replace with actual implementation):
        // api.RegisterFeature(
        //     "fse.forecast",
        //     helper.Translation.Get("fse.forecast.menu.tab.title"),
        //     () => { Game1.activeClickableMenu ??= forecastMenuService.CreateMenu(null); }
        // );
    }
}
