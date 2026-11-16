using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Air
{
    public sealed class AtmosphereConfig
    {
        public float GameSecondsPerSol { get; }
        public float InitialFraction { get; }
        public float YellowBelow { get; }
        public float RedBelow { get; }
        public float CriticalBelow { get; }
        public float RestKgPerSecond { get; }
        public float WorkKgPerSecond { get; }
        public float CriticalSeconds { get; }
        public float AirDensityKgPerM3 { get; }
        public float NewModuleFraction { get; }
        public float InitialVolumeM3 { get; }

        // Los consumos van por sol y se guardan por segundo de juego (T7).
        // criticalSeconds es tiempo real: es el margen de reacción del jugador.
        public AtmosphereConfig(float gameSecondsPerSol, float initialFraction,
            float yellowBelow, float redBelow, float criticalBelow,
            float restKgPerSol, float workKgPerSol, float criticalSeconds,
            float airDensityKgPerM3, float newModuleFraction, float initialVolumeM3)
        {
            ValidatePositive(gameSecondsPerSol, nameof(gameSecondsPerSol));
            ValidateFraction(initialFraction, nameof(initialFraction));
            ValidateFraction(yellowBelow, nameof(yellowBelow));
            ValidateFraction(redBelow, nameof(redBelow));
            ValidateFraction(criticalBelow, nameof(criticalBelow));
            if (!(yellowBelow > redBelow && redBelow > criticalBelow))
                throw new ArgumentOutOfRangeException(nameof(redBelow),
                    "Los límites deben cumplir amarillo > rojo > crítico.");
            ValidateAmount(restKgPerSol, nameof(restKgPerSol));
            ValidateAmount(workKgPerSol, nameof(workKgPerSol));
            ValidateAmount(criticalSeconds, nameof(criticalSeconds));
            ValidatePositive(airDensityKgPerM3, nameof(airDensityKgPerM3));
            if (float.IsNaN(newModuleFraction) || newModuleFraction < 0f || newModuleFraction > 1f)
                throw new ArgumentOutOfRangeException(nameof(newModuleFraction),
                    "La fracción debe estar entre 0 y 1.");
            ValidatePositive(initialVolumeM3, nameof(initialVolumeM3));

            GameSecondsPerSol = gameSecondsPerSol;
            InitialFraction = initialFraction;
            YellowBelow = yellowBelow;
            RedBelow = redBelow;
            CriticalBelow = criticalBelow;
            RestKgPerSecond = ToRatePerSecond(restKgPerSol);
            WorkKgPerSecond = ToRatePerSecond(workKgPerSol);
            CriticalSeconds = criticalSeconds;
            AirDensityKgPerM3 = airDensityKgPerM3;
            NewModuleFraction = newModuleFraction;
            InitialVolumeM3 = initialVolumeM3;
        }

        public static AtmosphereConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            var clock = new Clock.DayNightConfig(data.Get("clock.day_seconds"),
                data.Get("clock.night_seconds"), data.Get("clock.night_time_scale"));
            float seconds = (float)((double)clock.DaySeconds +
                (double)clock.NightSeconds * clock.NightTimeScale);
            return new AtmosphereConfig(seconds,
                data.Get("air.initial_fraction"),
                data.Get("air.band.yellow"),
                data.Get("air.band.red"),
                data.Get("air.band.critical"),
                data.Get("air.cat.rest_kg_per_sol"),
                data.Get("air.cat.work_kg_per_sol"),
                data.Get("air.critical_seconds"),
                data.Get("air.density"),
                data.Get("air.new_module_fraction"),
                data.Get("air.initial_volume_m3"));
        }

        internal float ToRatePerSecond(float ratePerSol)
        {
            ValidateAmount(ratePerSol, nameof(ratePerSol));
            double rate = (double)ratePerSol / GameSecondsPerSol;
            if (rate > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(ratePerSol),
                    "La tasa por segundo excede el rango de float.");
            return (float)rate;
        }

        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y mayor que cero.");
        }

        private static void ValidateAmount(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y no negativo.");
        }

        private static void ValidateFraction(float value, string name)
        {
            if (float.IsNaN(value) || value <= 0f || value > 1f)
                throw new ArgumentOutOfRangeException(name, "La fracción debe estar en (0, 1].");
        }
    }
}
