using System.Collections.Generic;
using System.Linq;
using fse.core.models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Machines;

namespace fse.core.services;

public interface IArtisanService
{
	void GenerateArtisanMapping(EconomyModel economyModel);
	ItemModel GetBaseFromArtisanGood(string modelId);
}

public class ArtisanService(IMonitor monitor, IModHelper helper) : IArtisanService
{
	private Dictionary<string, string> _artisanGoodToBase;
	private EconomyModel _economyModel;
	
	public void GenerateArtisanMapping(EconomyModel economyModel)
	{
		var machineData = helper.GameContent.Load<Dictionary<string, MachineData>>("Data\\Machines");

		if (machineData == null)
		{
			return;
		}

		_artisanGoodToBase = machineData.Values
			.Where(m => m.OutputRules != null)
			.SelectMany(m => m.OutputRules)
			.Where(r => r.OutputItem != null && r.Triggers != null)
			.SelectMany(r => r.OutputItem.SelectMany(output => r.Triggers.Select(trigger => (output, trigger))))
			.Select(t =>
			{
				t.trigger.RequiredItemId ??= t.trigger.RequiredTags?.Select(tag => ContextTagItemMapping.Mapping.GetValueOrDefault(tag))
					.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item));
				return t;
			})
			.Where(t => !string.IsNullOrWhiteSpace(t.trigger.RequiredItemId))
			.Where(t => !string.IsNullOrWhiteSpace(t.output.ItemId))
			.Select(t => (
				output: t.output.ItemId.Replace("(O)", string.Empty),
				input: t.trigger.RequiredItemId.Replace("(O)", string.Empty))
			)
			.Where(t => !t.input.Equals(t.output))
			.Where(t => !ArtisanMappingIgnoreList.IgnoreList.Contains(t.input))
			.Where(t => economyModel.HasItem(t.output) && economyModel.HasItem(t.input))
			.GroupBy(t => t.output)
			.Select(g =>
			{
				var first = g.First();
				return (first.output, first.input);
			})
			.ToDictionary(t => t.output, t => t.input);

		monitor.LogOnce("Artisan Good Mapping Trace");
		foreach (var mapping in _artisanGoodToBase)
		{
			monitor.LogOnce($"{mapping.Key} based on {mapping.Value}");
		}
		
		_economyModel = economyModel;
		
		BreakCycles();
	}

	public ItemModel GetBaseFromArtisanGood(string modelId)
	{
		if (_economyModel == null || _artisanGoodToBase == null)
		{
			return null;
		}

		if (!_artisanGoodToBase.ContainsKey(modelId))
		{
			return null;
		}

		var id = modelId;

		while (_artisanGoodToBase.TryGetValue(id, out var mappedItem))
		{
			id = mappedItem;
		}

		return _economyModel.GetItem(id);
	}

	private void BreakCycles()
	{
		var keys = _artisanGoodToBase.Keys.ToArray();

		foreach (var key in keys)
		{
			var id = key;
			var seen = new HashSet<string>{id};
			
			while (_artisanGoodToBase.TryGetValue(id, out var mappedItem))
			{
				if (!seen.Add(mappedItem))
				{
					var path = string.Join(" < ", seen) + " < " + mappedItem;
					monitor.LogOnce($"Artisan good cycle detected for {key}. Item economy will be unique. This may be intended by the mod author. Please report an issue if the following chain looks suspect.\n{path}", LogLevel.Warn);
					_artisanGoodToBase.Remove(key);
					break;
				}

				id = mappedItem;
			}
		}
	}
}