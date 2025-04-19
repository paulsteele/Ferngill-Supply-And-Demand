using System;
using System.Runtime.CompilerServices;
using StardewModdingAPI;

namespace fse.core.actions
{
	public static class SafeAction
	{
		public static void Run(Action action, IMonitor monitor, [CallerMemberName] string callerName = "")
		{
			Run(() =>
				{
					action.Invoke();
					return true;
				},
				true,
				monitor,
				callerName
			);
		}

		public static T Run<T>(Func<T> action, T defaultValue, IMonitor monitor, [CallerMemberName] string callerName = "")
		{
			try
			{
				return action.Invoke();
			}
			catch (Exception e)
			{
				monitor.Log(callerName, LogLevel.Error);
				monitor.Log(e.ToString(), LogLevel.Error);

				return defaultValue;
			}
		}
	}
}