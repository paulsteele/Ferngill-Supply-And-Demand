using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Tests.HarmonyMocks;

public static class HarmonyButtonPressedEventArgs
{
	public static ButtonPressedEventArgs CreateButtonPressedEventArgs
	(	
		SButton sButton,
		ICursorPosition cursorPosition
	)
	{
		return (ButtonPressedEventArgs)(typeof(ButtonPressedEventArgs)).Assembly.CreateInstance
		(
			typeof(ButtonPressedEventArgs).FullName, 
			false,
			BindingFlags.Instance | BindingFlags.NonPublic,
			null,
			new object[]
			{
				sButton,
				cursorPosition,
				(object)null
			},
			null,
			null
		);
	}
}