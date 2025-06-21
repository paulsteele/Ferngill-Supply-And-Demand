using System;
using System.Linq;
using fse.core.actions;
using fse.core.extensions;
using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using GenericModConfigMenu;

using Leclair.Stardew.BetterGameMenu;
using LeFauxMods.Common.Integrations.IconicFramework;
using MailFrameworkMod.Api;
using Microsoft.Xna.Framework;
using StarControl;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace fse.core.handlers;

public class GameLoadedHandler(
	IModHelper helper,
	IMonitor monitor,
	IManifest manifest,
	IBetterGameMenuService betterGameMenuService,
	IIconicFrameworkService iconicFrameworkService,
	IStarControlService starControlService,
	IGenericConfigMenuService genericConfigMenuService
) : IHandler
{
	public void Register()
	{
		helper.Events.GameLoop.GameLaunched += (_, _) => SafeAction.Run(OnLaunched, monitor, nameof(OnLaunched));
	}

	private void OnLaunched()
	{
		RegisterBetterGameMenu();
		RegisterIconicAndStarControl();
		RegisterMailFramework();
		RegisterGenericConfigMenu();
	}
	
	private void RegisterIconicAndStarControl()
	{
		var iconicFramework = helper.ModRegistry.GetApi<IIconicFrameworkApi>("furyx639.ToolbarIcons");
		var starControlApi = helper.ModRegistry.GetApi<IStarControlApi>("focustense.StarControl");
		
		iconicFrameworkService.Register(iconicFramework);
		starControlService.Register(starControlApi, iconicFramework);
	}

	private void RegisterBetterGameMenu()
	{
		var betterGameMenuApi = helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");
		betterGameMenuService.Register(betterGameMenuApi);
	}

	private void RegisterMailFramework()
	{
		var mailFrameworkModApi = helper.ModRegistry.GetApi<IMailFrameworkModApi>("DIGUS.MailFrameworkMod");
		if (mailFrameworkModApi == null)
		{
			return;
		}

		var contentPack = helper.ContentPacks.CreateTemporary($"{helper.DirectoryPath}/assets/mail", $"{helper.ModContent.ModID}.mail", "fsemail", "fsemail", "fse", manifest.Version);
		mailFrameworkModApi.RegisterContentPack(contentPack);
	}

	private void RegisterGenericConfigMenu()
	{
		var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
		genericConfigMenuService.Register(configMenu);
	}
}
