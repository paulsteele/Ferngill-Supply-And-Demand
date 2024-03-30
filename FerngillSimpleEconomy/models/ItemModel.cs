using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using fse.core.helpers;
using Object = StardewValley.Object;

namespace fse.core.models
{
	public class ItemModel
	{
		private int _dailyDelta;
		private int _supply;

		// there are a variety of factors that can influence sell price. Cache the calculation for each input
		private readonly Dictionary<int, int> _cachedPrices = new();

		[JsonInclude] public string ObjectId { get; set; }

		[JsonInclude]
		public int Supply
		{
			get => _supply;
			set => _supply = BoundsHelper.EnsureBounds(value, ConfigModel.MinSupply, ConfigModel.MaxSupply);
		}

		[JsonInclude]
		public int DailyDelta
		{
			get => _dailyDelta;
			set => _dailyDelta = BoundsHelper.EnsureBounds(value, ConfigModel.Instance.MinDelta, ConfigModel.Instance.MaxDelta);
		}

		public void AdvanceOneDay()
		{
			Supply += DailyDelta;
			_cachedPrices.Clear();
		}

		public float GetMultiplier()
		{
			var ratio = 1 - (Math.Min(Supply, ConfigModel.Instance.MaxCalculatedSupply) / (float)ConfigModel.Instance.MaxCalculatedSupply);
			var percentageRange = ConfigModel.Instance.MaxPercentage - ConfigModel.Instance.MinPercentage;

			return (ratio * percentageRange) + ConfigModel.Instance.MinPercentage;
		}

		public int GetPrice(int basePrice)
		{
			// ReSharper disable once InvertIf
			if (!_cachedPrices.ContainsKey(basePrice))
			{
				_cachedPrices.Add(basePrice, (int)(basePrice * GetMultiplier()));
			}

			return _cachedPrices[basePrice];
		}

		public void CapSupply()
		{
			Supply = Math.Min(Supply, ConfigModel.Instance.MaxCalculatedSupply);
		}
		
		private Object _objectInstance;
		public Object GetObjectInstance() => _objectInstance ??= new Object(ObjectId, 1);
	}
}