using StardewModdingAPI.Events;

namespace Tests.Mocks;

public class MockGameLoopEvents : IGameLoopEvents
{
	public event EventHandler<GameLaunchedEventArgs>? GameLaunched;
	public event EventHandler<UpdateTickingEventArgs>? UpdateTicking;
	public event EventHandler<UpdateTickedEventArgs>? UpdateTicked;
	public event EventHandler<OneSecondUpdateTickingEventArgs>? OneSecondUpdateTicking;
	public event EventHandler<OneSecondUpdateTickedEventArgs>? OneSecondUpdateTicked;
	public event EventHandler<SaveCreatingEventArgs>? SaveCreating;
	public event EventHandler<SaveCreatedEventArgs>? SaveCreated;
	public event EventHandler<SavingEventArgs>? Saving;
	public event EventHandler<SavedEventArgs>? Saved;
	public event EventHandler<SaveLoadedEventArgs>? SaveLoaded;
	public event EventHandler<DayStartedEventArgs>? DayStarted;
	public event EventHandler<DayEndingEventArgs>? DayEnding;
	public event EventHandler<TimeChangedEventArgs>? TimeChanged;
	public event EventHandler<ReturnedToTitleEventArgs>? ReturnedToTitle;

	public void InvokeDayStarted()
	{
		DayStarted?.Invoke(this, new DayStartedEventArgs());
	}
	
	public void InvokeDayEnding()
	{
		DayEnding?.Invoke(this, new DayEndingEventArgs());
	}

	public void InvokeSaveLoaded()
	{
		SaveLoaded?.Invoke(this, new SaveLoadedEventArgs());
	}
}