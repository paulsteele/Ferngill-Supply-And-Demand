using StardewModdingAPI.Events;

namespace Tests.Mocks;

public class MockDisplayEvents : IDisplayEvents
{
	public event EventHandler<MenuChangedEventArgs>? MenuChanged;
	public event EventHandler<RenderingStepEventArgs>? RenderingStep;
	public event EventHandler<RenderedStepEventArgs>? RenderedStep;
	public event EventHandler<RenderingEventArgs>? Rendering;
	public event EventHandler<RenderedEventArgs>? Rendered;
	public event EventHandler<RenderingWorldEventArgs>? RenderingWorld;
	public event EventHandler<RenderedWorldEventArgs>? RenderedWorld;
	public event EventHandler<RenderingActiveMenuEventArgs>? RenderingActiveMenu;
	public event EventHandler<RenderedActiveMenuEventArgs>? RenderedActiveMenu;
	public event EventHandler<RenderingHudEventArgs>? RenderingHud;
	public event EventHandler<RenderedHudEventArgs>? RenderedHud;
	public event EventHandler<WindowResizedEventArgs>? WindowResized;

	public void InvokeRenderedActiveMenu(RenderedActiveMenuEventArgs args)
	{
		RenderedActiveMenu?.Invoke(this, args);
	}
}