using System;
using fse.core.helpers;
using fse.core.menu;
using StardewModdingAPI;

namespace fse.core.services;

public interface IForecastMenuService
{
	AbstractForecastMenu CreateMenu(Action? exitAction);
}

public class ForecastMenuService(
	IModHelper modHelper, 
	IEconomyService economyService,
	IDrawTextHelper drawTextHelper
) : IForecastMenuService
{
	public AbstractForecastMenu CreateMenu(Action? exitAction) => new ForecastMenu(modHelper, economyService, drawTextHelper, exitAction);
}