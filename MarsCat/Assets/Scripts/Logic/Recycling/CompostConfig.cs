using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Recycling
{
    // Parámetros de la Sopa de Bacterias. Tiempos en segundos reales; temperatura en °C; humedad en %.
    public sealed class CompostConfig
    {
        public float TempGreenMin { get; }
        public float TempGreenMax { get; }
        public float OverheatTemp { get; }
        public float HumidityGreenMin { get; }
        public float HumidityGreenMax { get; }
        public float DrownHumidity { get; }
        public float DryHumidity { get; }
        public float CoolingPerSecond { get; }
        public float Inertia { get; }
        public float HeatKwhPerSecond { get; }
        public float SprinklerLitersPerSecond { get; }
        public float CycleSeconds { get; }
        public float BatchKg { get; }
        public float YieldRatio { get; }
        public float StartTemp { get; }
        public float AmbientTemp { get; }
        public float MaxHeatTemp { get; }
        public float StartHumidity { get; }
        public float SprinklerHumidityPerSecond { get; }
        public float DryingPerSecond { get; }

        public CompostConfig(float tempGreenMin, float tempGreenMax, float overheatTemp,
            float humidityGreenMin, float humidityGreenMax, float drownHumidity, float dryHumidity,
            float coolingPerSecond, float inertia, float heatKwhPerSecond, float sprinklerLitersPerSecond,
            float cycleSeconds, float batchKg, float yieldRatio,
            float startTemp, float ambientTemp, float maxHeatTemp,
            float startHumidity, float sprinklerHumidityPerSecond, float dryingPerSecond)
        {
            if (!(tempGreenMin < tempGreenMax) || !(tempGreenMax < overheatTemp))
                throw new ArgumentException("Debe cumplirse zona verde mínima < máxima < sobrecalentamiento.");
            if (!(dryHumidity < humidityGreenMin) || !(humidityGreenMin < humidityGreenMax) || !(humidityGreenMax < drownHumidity))
                throw new ArgumentException("Debe cumplirse sequedad < verde mínima < verde máxima < ahogo.");
            if (!(ambientTemp <= maxHeatTemp))
                throw new ArgumentException("La temperatura ambiente no puede superar la del calor al máximo.");
            ValidateNonNegative(coolingPerSecond, nameof(coolingPerSecond));
            ValidateNonNegative(inertia, nameof(inertia));
            ValidateNonNegative(heatKwhPerSecond, nameof(heatKwhPerSecond));
            ValidateNonNegative(sprinklerLitersPerSecond, nameof(sprinklerLitersPerSecond));
            ValidateNonNegative(batchKg, nameof(batchKg));
            ValidateNonNegative(yieldRatio, nameof(yieldRatio));
            ValidateNonNegative(sprinklerHumidityPerSecond, nameof(sprinklerHumidityPerSecond));
            ValidateNonNegative(dryingPerSecond, nameof(dryingPerSecond));
            if (float.IsNaN(cycleSeconds) || float.IsInfinity(cycleSeconds) || cycleSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cycleSeconds), "El ciclo debe durar un tiempo finito y positivo.");
            ValidateFinite(startTemp, nameof(startTemp));
            ValidateFinite(ambientTemp, nameof(ambientTemp));
            ValidateFinite(maxHeatTemp, nameof(maxHeatTemp));
            if (float.IsNaN(startHumidity) || float.IsInfinity(startHumidity) || startHumidity < 0f || startHumidity > 100f)
                throw new ArgumentOutOfRangeException(nameof(startHumidity), "La humedad inicial debe estar entre 0 y 100.");

            TempGreenMin = tempGreenMin;
            TempGreenMax = tempGreenMax;
            OverheatTemp = overheatTemp;
            HumidityGreenMin = humidityGreenMin;
            HumidityGreenMax = humidityGreenMax;
            DrownHumidity = drownHumidity;
            DryHumidity = dryHumidity;
            CoolingPerSecond = coolingPerSecond;
            Inertia = inertia;
            HeatKwhPerSecond = heatKwhPerSecond;
            SprinklerLitersPerSecond = sprinklerLitersPerSecond;
            CycleSeconds = cycleSeconds;
            BatchKg = batchKg;
            YieldRatio = yieldRatio;
            StartTemp = startTemp;
            AmbientTemp = ambientTemp;
            MaxHeatTemp = maxHeatTemp;
            StartHumidity = startHumidity;
            SprinklerHumidityPerSecond = sprinklerHumidityPerSecond;
            DryingPerSecond = dryingPerSecond;
        }

        public static CompostConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            const string p = "minigame.compost.";
            return new CompostConfig(
                data.Get(p + "temp.green_min"), data.Get(p + "temp.green_max"), data.Get(p + "temp.overheat"),
                data.Get(p + "humidity.green_min"), data.Get(p + "humidity.green_max"),
                data.Get(p + "humidity.drown"), data.Get(p + "humidity.dry"),
                data.Get(p + "cooling_per_second"), data.Get(p + "inertia"),
                data.Get(p + "heat_kwh_per_second"), data.Get(p + "sprinkler_l_per_second"),
                data.Get(p + "cycle_seconds"), data.Get(p + "batch_kg"), data.Get(p + "yield_ratio"),
                data.Get(p + "temp.start"), data.Get(p + "temp.ambient"), data.Get(p + "temp.max_heat"),
                data.Get(p + "humidity.start"), data.Get(p + "humidity.sprinkler_per_second"),
                data.Get(p + "humidity.dry_per_second"));
        }

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y no negativo.");
        }

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito.");
        }
    }
}
