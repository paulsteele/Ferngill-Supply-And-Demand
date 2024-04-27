using fse.core.helpers;
using fse.core.menu;
using StardewModdingAPI;

namespace fse.core.services;

public interface IForecastMenuService
{
	AbstractForecastMenu CreateMenu();
}

public class ForecastMenuService(
	IModHelper modHelper, 
	IEconomyService economyService, 
	IMonitor monitor,
	IDrawTextHelper drawTextHelper
) : IForecastMenuService
{
	public AbstractForecastMenu CreateMenu() => new ForecastMenu(modHelper, economyService, monitor, drawTextHelper);
}