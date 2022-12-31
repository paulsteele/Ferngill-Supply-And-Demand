using System;

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
        
        public int ObjectId { get; set; }
        private int _supply;

        public int Supply
        {
            get => _supply;
            set => _supply = EnsureBounds(value, MinSupply, MaxSupply);
        }

        private int _dailyDelta;

        public int DailyDelta
        {
            get => _dailyDelta;
            set => _dailyDelta = EnsureBounds(value, MinDelta, MaxDelta);
        } 

        private static int EnsureBounds(int input, int min, int max)
        {
            return Math.Min(Math.Max(input, min), max);
        }
    }
}