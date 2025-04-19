using System;
using fse.core.helpers;
using fse.core.menu;
using StardewModdingAPI;
using StardewValley.Menus;

namespace fse.core.services;

public interface IForecastMenuService
{
	ForecastMenu CreateMenu(Action? exitAction);
}

public class ForecastMenuService(
	IModHelper modHelper, 
	IEconomyService economyService,
	IDrawTextHelper drawTextHelper,
	IDrawSupplyBarHelper drawSupplyBarHelper
) : IForecastMenuService
{
	public ForecastMenu CreateMenu(Action? exitAction) => new ForecastMenu(modHelper, economyService, drawTextHelper, drawSupplyBarHelper, exitAction);
}