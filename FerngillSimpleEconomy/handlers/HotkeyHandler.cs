using fse.core.models;
using fse.core.services;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace fse.core.handlers;

public class HotkeyHandler(IModHelper helper, IForecastMenuService forecastMenuService) : IHandler
{
	public void Register()
	{
		helper.Events.Input.ButtonsChanged += InputOnButtonPressed;
	}

	private void InputOnButtonPressed(object? sender, ButtonsChangedEventArgs e)
	{
		if (!Context.IsPlayerFree)
		{
			return;
		}
		if (!ConfigModel.Instance.ShowMenuHotkey.JustPressed())
		{
			return;
		}
		
		Game1.activeClickableMenu ??= forecastMenuService.CreateMenu(null);
	}
}