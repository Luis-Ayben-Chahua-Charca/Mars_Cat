using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Food
{
    public sealed class RatHuntConfig
    {
        // Segundos reales que tarda un gato hambriento en cazar.
        public float HuntSeconds { get; }
        // Raciones de una comida: con menos que esto en el depósito el gato no puede comer.
        public float FoodRation { get; }

        public RatHuntConfig(float huntSeconds, float foodRation)
        {
            ValidatePositive(huntSeconds, nameof(huntSeconds));
            ValidatePositive(foodRation, nameof(foodRation));
            HuntSeconds = huntSeconds;
            FoodRation = foodRation;
        }

        public static RatHuntConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return new RatHuntConfig(data.Get("rat.hunt_seconds"), data.Get("cat.food.ration"));
        }

        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y mayor que cero.");
        }
    }
}
