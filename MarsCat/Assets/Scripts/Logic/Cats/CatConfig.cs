// Asistencia de IA: escrito con Codex (OpenAI); modificado con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Cats
{
    public sealed class CatConfig
    {
        public float FoodIntervalSeconds { get; }
        public float FoodAlertSeconds { get; }
        public float FoodDeathSeconds { get; }
        public float FoodRation { get; }
        public float WaterIntervalSeconds { get; }
        public float WaterAlertSeconds { get; }
        public float WaterDeathSeconds { get; }
        public float WaterRation { get; }
        public float InitialNeedSpread { get; }
        public float WastePerMeal { get; }

        public CatConfig(float foodIntervalSeconds, float foodAlertSeconds,
            float foodDeathSeconds, float foodRation, float waterIntervalSeconds,
            float waterAlertSeconds, float waterDeathSeconds, float waterRation,
            float initialNeedSpread, float wastePerMeal = 0f)
        {
            ValidateNeed(foodIntervalSeconds, foodAlertSeconds, foodDeathSeconds, foodRation, "food");
            ValidateNeed(waterIntervalSeconds, waterAlertSeconds, waterDeathSeconds, waterRation, "water");
            if (float.IsNaN(initialNeedSpread) || float.IsInfinity(initialNeedSpread)
                || initialNeedSpread < 0f || initialNeedSpread >= 1f)
                throw new ArgumentOutOfRangeException(nameof(initialNeedSpread),
                    "La dispersión inicial debe ser finita y estar en [0, 1).");

            FoodIntervalSeconds = foodIntervalSeconds;
            FoodAlertSeconds = foodAlertSeconds;
            FoodDeathSeconds = foodDeathSeconds;
            FoodRation = foodRation;
            WaterIntervalSeconds = waterIntervalSeconds;
            WaterAlertSeconds = waterAlertSeconds;
            WaterDeathSeconds = waterDeathSeconds;
            WaterRation = waterRation;
            if (float.IsNaN(wastePerMeal) || float.IsInfinity(wastePerMeal) || wastePerMeal < 0f)
                throw new ArgumentOutOfRangeException(nameof(wastePerMeal),
                    "Los desechos por comida deben ser finitos y no negativos.");

            InitialNeedSpread = initialNeedSpread;
            WastePerMeal = wastePerMeal;
        }

        public static CatConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            // Si la clave no existe no se generan desechos (compatibilidad con balances anteriores).
            data.TryGet("cat.waste_per_meal", out float wastePerMeal);
            return new CatConfig(data.Get("cat.food.interval_seconds"),
                data.Get("cat.food.alert_seconds"), data.Get("cat.food.death_seconds"),
                data.Get("cat.food.ration"), data.Get("cat.water.interval_seconds"),
                data.Get("cat.water.alert_seconds"), data.Get("cat.water.death_seconds"),
                data.Get("cat.water.ration"), data.Get("cat.initial_need_spread"), wastePerMeal);
        }

        private static void ValidateNeed(float interval, float alert, float death, float ration, string name)
        {
            ValidatePositive(interval, name + "IntervalSeconds");
            ValidatePositive(alert, name + "AlertSeconds");
            ValidatePositive(death, name + "DeathSeconds");
            ValidatePositive(ration, name + "Ration");
            if (interval >= alert || alert >= death)
                throw new ArgumentException("Los tiempos deben cumplir intervalo < alerta < muerte: " + name + ".");
        }

        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y mayor que cero.");
        }
    }
}
