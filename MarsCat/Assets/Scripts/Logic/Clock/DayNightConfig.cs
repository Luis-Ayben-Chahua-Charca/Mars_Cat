// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using MarsCat.Logic.Data;

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

        public static DayNightConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return new DayNightConfig(data.Get("clock.day_seconds"),
                data.Get("clock.night_seconds"), data.Get("clock.night_time_scale"),
                data.Get("clock.start_progress"));
        }

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
