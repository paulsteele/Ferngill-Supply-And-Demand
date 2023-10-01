using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

// ReSharper disable once CheckNamespace
namespace MailFrameworkMod.Api
{
	public interface IMailFrameworkModApi
	{
		public void RegisterContentPack(IContentPack contentPack);

		public void RegisterLetter(ILetter iLetter, Func<ILetter, bool> condition, Action<ILetter> callback = null, Func<ILetter, List<Item>> dynamicItems = null);

		public ILetter GetLetter(string id);

		public string GetMailDataString(string id);
	}
}