using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace Tests.Mocks;

public class MockTranslationHelper : ITranslationHelper
{
	public string ModID { get; }
	public IEnumerable<Translation> GetTranslations() => throw new NotImplementedException();

	public Translation Get(string key)
	{
		return (Translation)(typeof(Translation)).Assembly.CreateInstance
		(
			typeof(Translation).FullName, 
			false,
			BindingFlags.Instance | BindingFlags.NonPublic,
			null,
			new[]
			{
				"test-locale",
				key,
				$"translation-{key}",
			},
			null,
			null
		);
	}

	public Translation Get(string key, object? tokens) => throw new NotImplementedException();

	public IDictionary<string, Translation> GetInAllLocales(string key, bool withFallback = false) => throw new NotImplementedException();

	public string Locale { get; }
	public LocalizedContentManager.LanguageCode LocaleEnum { get; }
}