using System;
using fse.core.helpers;
using fse.core.menu;
using StardewModdingAPI;

namespace fse.core.services;

public interface IForecastMenuService
{
	IForecastMenu CreateMenu(Action? exitAction);
}

public class ForecastMenuService(
	IModHelper modHelper, 
	IEconomyService economyService,
	IDrawTextHelper drawTextHelper,
	IDrawSupplyBarHelper drawSupplyBarHelper
) : IForecastMenuService
{
	public IForecastMenu CreateMenu(Action? exitAction) => new ForecastMenu(modHelper, economyService, drawTextHelper, drawSupplyBarHelper, exitAction);
}