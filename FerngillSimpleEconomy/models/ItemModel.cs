using System;
using System.Text.Json.Serialization;
using fse.core.helpers;
using Object = StardewValley.Object;

namespace fse.core.models
{
	[method: JsonConstructor]
	public class ItemModel(string objectId)
	{
		private int _dailyDelta;
		private int _supply;

		//cache the multiplier at the beginning of the day to keep prices consistent throughout the day
		private double _cachedMultiplier = 1d;

		[JsonInclude] public string ObjectId => objectId;

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
			UpdateMultiplier();
		}

		public void UpdateMultiplier()
		{
			_cachedMultiplier = GetMultiplier();
		}

		private double GetMultiplier()
		{
			var ratio = 1 - (Math.Min(Supply, ConfigModel.Instance.MaxCalculatedSupply) / (double)ConfigModel.Instance.MaxCalculatedSupply);
			var percentageRange = ConfigModel.Instance.MaxPercentage - ConfigModel.Instance.MinPercentage;

			return (ratio * percentageRange) + ConfigModel.Instance.MinPercentage;
		}

		public int GetPrice(int basePrice) => (int)(basePrice * _cachedMultiplier);

		public void CapSupply()
		{
			Supply = Math.Min(Supply, ConfigModel.Instance.MaxCalculatedSupply);
		}
		
		private Object? _objectInstance;
		public Object GetObjectInstance() => _objectInstance ??= new Object(ObjectId, 1);
	}
}