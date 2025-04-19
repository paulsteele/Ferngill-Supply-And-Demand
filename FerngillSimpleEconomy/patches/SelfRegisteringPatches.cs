using System;
using fse.core.helpers;
using fse.core.services;
using HarmonyLib;
using StardewModdingAPI;

namespace fse.core.patches
{
	public abstract class SelfRegisteringPatches
	{
		private static IMonitor? _monitor;
		protected static IMonitor Monitor => _monitor ?? throw new Exception("Monitor not initialized");
		
		private static IEconomyService? _economyService;
		protected static IEconomyService EconomyService => _economyService ?? throw new Exception("EconomyService not initialized");
		
		private static IDrawSupplyBarHelper? _drawSupplyBarHelper;
		protected static IDrawSupplyBarHelper DrawSupplyBarHelper => _drawSupplyBarHelper ?? throw new Exception("DrawSupplyBarHelper not initialized");

		public static void Initialize(
			IEconomyService economyService, 
			IMonitor monitor,
			IDrawSupplyBarHelper drawSupplyBarHelper
		)
		{
			_economyService = economyService;
			_monitor = monitor;
			_drawSupplyBarHelper = drawSupplyBarHelper;
		}

		public abstract void Register(Harmony harmony);
	}
}