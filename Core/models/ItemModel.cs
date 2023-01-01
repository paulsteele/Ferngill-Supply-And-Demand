using System;
using System.Text.Json.Serialization;

namespace fsd.core.models
{
    public class ItemModel
    {
        private const int MinSupply = 0;
        private const int MaxSupply = 1000;
        public const int StdDevSupply = 150;
        private const int MinDelta = -50;
        private const int MaxDelta = 50;
        public const int StdDevDelta = 15;

        public static int MeanSupply => (MinSupply + MaxSupply) / 2;
        public static int MeanDelta => (MinDelta + MaxDelta) / 2;
        
        [JsonInclude]
        public int ObjectId { get; set; }
        private int _supply;

        [JsonInclude]
        public int Supply
        {
            get => _supply;
            set => _supply = EnsureBounds(value, MinSupply, MaxSupply);
        }

        private int _dailyDelta;

        [JsonInclude]
        public int DailyDelta
        {
            get => _dailyDelta;
            set => _dailyDelta = EnsureBounds(value, MinDelta, MaxDelta);
        }

        public void AdvanceOneDay()
        {
            Supply += DailyDelta;
        }

        private static int EnsureBounds(int input, int min, int max)
        {
            return Math.Min(Math.Max(input, min), max);
        }
    }
}