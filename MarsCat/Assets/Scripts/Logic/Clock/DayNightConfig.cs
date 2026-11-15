using System;

namespace MarsCat.Logic.Clock
{
    public sealed class DayNightConfig
    {
        public float DaySeconds { get; }
        public float NightSeconds { get; }
        public float NightTimeScale { get; }
        public float StartProgress { get; }

        public DayNightConfig(float daySeconds, float nightSeconds,
            float nightTimeScale, float startProgress = 0f)
        {
            ValidatePositive(daySeconds, nameof(daySeconds));
            ValidatePositive(nightSeconds, nameof(nightSeconds));
            ValidatePositive(nightTimeScale, nameof(nightTimeScale));

            if (float.IsNaN(startProgress) || float.IsInfinity(startProgress)
                || startProgress < 0f || startProgress >= 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(startProgress),
                    "El progreso inicial debe ser finito y estar en [0, 1).");
            }

            if ((double)daySeconds + (double)nightSeconds * nightTimeScale > float.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(nightTimeScale),
                    "La duración del sol en tiempo de juego excede el rango de float.");
            }

            DaySeconds = daySeconds;
            NightSeconds = nightSeconds;
            NightTimeScale = nightTimeScale;
            StartProgress = startProgress;
        }

        // FromBalance(BalanceData) queda pendiente hasta disponer del contrato implementado en #22.

        private static void ValidatePositive(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName,
                    "El valor debe ser finito y mayor que cero.");
            }
        }
    }
}
