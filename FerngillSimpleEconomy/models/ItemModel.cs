using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using fse.core.helpers;
using Object = StardewValley.Object;

namespace fse.core.models
{
	public class ItemModel
	{
		private const int MinSupply = 0;
		public const int MaxCalculatedSupply = 1000;
		private const int MaxSupply = int.MaxValue;
		public const int StdDevSupply = 150;
		private const int MinDelta = -30;
		private const int MaxDelta = 30;
		public const int StdDevDelta = 5;

		private const float MaxPercentage = 1.3f;
		private const float MinPercentage = 0.2f;

		private int _dailyDelta;

		private int _supply;

		// there are a variety of factors that can influence sell price. Cache the calculation for each input
		private readonly Dictionary<int, int> _cachedPrices = new();

		public static int MeanSupply => (MinSupply + MaxCalculatedSupply) / 2;
		public static int MeanDelta => (MinDelta + MaxDelta) / 2;

		[JsonInclude] public int ObjectId { get; set; }

		[JsonInclude]
		public int Supply
		{
			get => _supply;
			set => _supply = BoundsHelper.EnsureBounds(value, MinSupply, MaxSupply);
		}

		[JsonInclude]
		public int DailyDelta
		{
			get => _dailyDelta;
			set => _dailyDelta = BoundsHelper.EnsureBounds(value, MinDelta, MaxDelta);
		}

		public void AdvanceOneDay()
		{
			Supply += DailyDelta;
			_cachedPrices.Clear();
		}

		public float GetMultiplier()
		{
			var ratio = 1 - (Math.Min(Supply, MaxCalculatedSupply) / (float)MaxCalculatedSupply);
			const float percentageRange = MaxPercentage - MinPercentage;

			return (ratio * percentageRange) + MinPercentage;
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
			Supply = Math.Min(Supply, MaxCalculatedSupply);
		}
		
		private Object _objectInstance;
		public Object GetObjectInstance() => _objectInstance ??= new Object(ObjectId, 1);
	}
}