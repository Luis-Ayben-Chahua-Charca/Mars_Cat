using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Energy
{
    public sealed class EnergyConfig
    {
        public float GameSecondsPerSol { get; }
        public float HeatingRatePerSecond { get; }

        public EnergyConfig(float gameSecondsPerSol, float heatingRatePerSol)
        {
            ValidateAmount(gameSecondsPerSol, nameof(gameSecondsPerSol));
            if (gameSecondsPerSol == 0f)
                throw new ArgumentOutOfRangeException(nameof(gameSecondsPerSol));
            ValidateAmount(heatingRatePerSol, nameof(heatingRatePerSol));
            GameSecondsPerSol = gameSecondsPerSol;
            HeatingRatePerSecond = ToRatePerSecond(heatingRatePerSol);
        }

        public static EnergyConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            var clock = new Clock.DayNightConfig(data.Get("clock.day_seconds"),
                data.Get("clock.night_seconds"), data.Get("clock.night_time_scale"));
            float seconds = (float)((double)clock.DaySeconds +
                (double)clock.NightSeconds * clock.NightTimeScale);
            return new EnergyConfig(seconds, data.Get("energy.heating.rate_per_sol"));
        }

        internal float ToRatePerSecond(float ratePerSol)
        {
            ValidateAmount(ratePerSol, nameof(ratePerSol));
            double rate = (double)ratePerSol / GameSecondsPerSol;
            if (rate > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(ratePerSol), "La tasa por segundo excede el rango de float.");
            return (float)rate;
        }

        internal static void ValidateAmount(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y no negativo.");
        }
    }
}
