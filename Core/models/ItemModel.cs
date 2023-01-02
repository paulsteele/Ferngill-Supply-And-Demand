using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace fsd.core.models
{
	public class ItemModel
	{
		private const int MinSupply = 0;
		private const int MaxCalculatedSupply = 1000;
		private const int MaxSupply = int.MaxValue;
		public const int StdDevSupply = 150;
		private const int MinDelta = -50;
		private const int MaxDelta = 50;
		public const int StdDevDelta = 15;

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
			set => _supply = EnsureBounds(value, MinSupply, MaxSupply);
		}

		[JsonInclude]
		public int DailyDelta
		{
			get => _dailyDelta;
			set => _dailyDelta = EnsureBounds(value, MinDelta, MaxDelta);
		}

		public void AdvanceOneDay()
		{
			Supply += DailyDelta;
			_cachedPrices.Clear();
		}

		private static int EnsureBounds(int input, int min, int max) => Math.Min(Math.Max(input, min), max);

		public int GetPrice(int basePrice)
		{
			// ReSharper disable once InvertIf
			if (!_cachedPrices.ContainsKey(basePrice))
			{
				var ratio = 1 - (Math.Min(Supply, MaxCalculatedSupply) / (float)MaxCalculatedSupply);
				const float percentageRange = MaxPercentage - MinPercentage;

				var multiplier = (ratio * percentageRange) + MinPercentage;

				_cachedPrices.Add(basePrice, (int)(basePrice * multiplier));
			}
			
			return _cachedPrices[basePrice];
		}

		public void CapSupply()
		{
			Supply = MaxCalculatedSupply;
		}
	}
}