using StardewModdingAPI.Events;

namespace Tests.Mocks;

public class MockInputEvents : IInputEvents
{
	public event EventHandler<ButtonsChangedEventArgs>? ButtonsChanged;
	public event EventHandler<ButtonPressedEventArgs>? ButtonPressed;
	public event EventHandler<ButtonReleasedEventArgs>? ButtonReleased;
	public event EventHandler<CursorMovedEventArgs>? CursorMoved;
	public event EventHandler<MouseWheelScrolledEventArgs>? MouseWheelScrolled;

	public void InvokeButtonPressed(ButtonPressedEventArgs args)
	{
		ButtonPressed?.Invoke(this, args);
	}
}