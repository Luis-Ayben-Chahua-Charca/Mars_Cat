using System;
using MarsCat.Logic.Clock;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Food
{
    public sealed class RatFarmConfig
    {
        public float RationsPerRat { get; }
        // Segundos de juego que tarda la conversión de una rata en raciones.
        public float ConvertSeconds { get; }
        public float GameSecondsPerSol { get; }
        public float ForagePerRatPerSecond { get; }
        public float WastePerRatPerSecond { get; }
        public int MaxRats { get; }
        public int MinBreeders { get; }
        public float BreedRatePerSecond { get; }

        // Las tasas "por sol" se convierten aquí a "por segundo de juego" (T7).
        public RatFarmConfig(float rationsPerRat, float convertSeconds, float foragePerRatPerSol,
            float wastePerRatPerSol, int maxRats, int minBreeders, float breedRatePerSol,
            float gameSecondsPerSol)
        {
            ValidatePositive(rationsPerRat, nameof(rationsPerRat));
            ValidatePositive(convertSeconds, nameof(convertSeconds));
            ValidatePositive(gameSecondsPerSol, nameof(gameSecondsPerSol));
            ValidateNonNegative(foragePerRatPerSol, nameof(foragePerRatPerSol));
            ValidateNonNegative(wastePerRatPerSol, nameof(wastePerRatPerSol));
            ValidateNonNegative(breedRatePerSol, nameof(breedRatePerSol));
            if (maxRats <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxRats), "El tope de ratas debe ser positivo.");
            if (minBreeders < 0 || minBreeders > maxRats)
                throw new ArgumentOutOfRangeException(nameof(minBreeders),
                    "El pie de cría debe estar entre 0 y el tope de ratas.");

            RationsPerRat = rationsPerRat;
            ConvertSeconds = convertSeconds;
            GameSecondsPerSol = gameSecondsPerSol;
            ForagePerRatPerSecond = ToPerSecond(foragePerRatPerSol, gameSecondsPerSol, nameof(foragePerRatPerSol));
            WastePerRatPerSecond = ToPerSecond(wastePerRatPerSol, gameSecondsPerSol, nameof(wastePerRatPerSol));
            BreedRatePerSecond = ToPerSecond(breedRatePerSol, gameSecondsPerSol, nameof(breedRatePerSol));
            MaxRats = maxRats;
            MinBreeders = minBreeders;
        }

        public static RatFarmConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            var clock = new DayNightConfig(data.Get("clock.day_seconds"),
                data.Get("clock.night_seconds"), data.Get("clock.night_time_scale"));
            float seconds = (float)((double)clock.DaySeconds +
                (double)clock.NightSeconds * clock.NightTimeScale);
            return new RatFarmConfig(data.Get("building.rat_farm.rations_per_rat"),
                data.Get("building.rat_farm.convert_seconds"),
                data.Get("building.rat_farm.forage_per_rat_per_sol"),
                data.Get("building.rat_farm.waste_per_rat_per_sol"),
                ReadInt(data, "building.rat_farm.max_rats"),
                ReadInt(data, "building.rat_farm.min_breeders"),
                data.Get("building.rat_farm.breed_rate"), seconds);
        }

        internal static int ReadInt(BalanceData data, string key)
        {
            float value = data.Get(key);
            if (float.IsNaN(value) || value < 0f || value > int.MaxValue || value != Math.Floor(value))
                throw new ArgumentOutOfRangeException(nameof(data),
                    "La clave '" + key + "' debe ser un entero no negativo.");
            return (int)value;
        }

        private static float ToPerSecond(float perSol, float secondsPerSol, string name)
        {
            double rate = (double)perSol / secondsPerSol;
            if (rate > float.MaxValue)
                throw new ArgumentOutOfRangeException(name, "La tasa por segundo excede el rango de float.");
            return (float)rate;
        }

        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y mayor que cero.");
        }

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y no negativo.");
        }
    }
}
