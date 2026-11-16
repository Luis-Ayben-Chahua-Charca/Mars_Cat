using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Water
{
    public sealed class MeltingConfig
    {
        public float InitialProgress { get; }
        public float FillSeconds { get; }
        public float DrainPerSecond { get; }
        public float GreenWidth { get; }
        public float Oscillation { get; }
        public float AccelerateFactor { get; }
        public float AccelerateExtraKwh { get; }

        // Todos los tiempos son segundos reales (es interacción del jugador).
        public MeltingConfig(float initialProgress, float fillSeconds, float drainPerSecond,
            float greenWidth, float oscillation, float accelerateFactor, float accelerateExtraKwh)
        {
            if (!IsFinite(initialProgress) || initialProgress <= 0f || initialProgress >= 1f)
                throw new ArgumentOutOfRangeException(nameof(initialProgress), "El progreso inicial debe estar entre 0 y 1 (sin incluirlos).");
            if (!IsFinite(fillSeconds) || fillSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(fillSeconds), "Los segundos para llenar deben ser positivos.");
            if (!IsFinite(drainPerSecond) || drainPerSecond < 0f)
                throw new ArgumentOutOfRangeException(nameof(drainPerSecond), "La bajada no puede ser negativa.");
            if (!IsFinite(greenWidth) || greenWidth <= 0f || greenWidth > 1f)
                throw new ArgumentOutOfRangeException(nameof(greenWidth), "El ancho de la zona verde debe estar entre 0 (sin incluirlo) y 1.");
            if (!IsFinite(oscillation) || oscillation < 0f)
                throw new ArgumentOutOfRangeException(nameof(oscillation), "La oscilación no puede ser negativa.");
            if (!IsFinite(accelerateFactor) || accelerateFactor < 1f)
                throw new ArgumentOutOfRangeException(nameof(accelerateFactor), "El factor de aceleración debe ser al menos 1.");
            if (!IsFinite(accelerateExtraKwh) || accelerateExtraKwh < 0f)
                throw new ArgumentOutOfRangeException(nameof(accelerateExtraKwh), "La electricidad extra no puede ser negativa.");
            InitialProgress = initialProgress;
            FillSeconds = fillSeconds;
            DrainPerSecond = drainPerSecond;
            GreenWidth = greenWidth;
            Oscillation = oscillation;
            AccelerateFactor = accelerateFactor;
            AccelerateExtraKwh = accelerateExtraKwh;
        }

        public static MeltingConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return new MeltingConfig(
                data.Get("minigame.melt.initial_progress"),
                data.Get("minigame.melt.fill_seconds"),
                data.Get("minigame.melt.drain_per_second"),
                data.Get("minigame.melt.green_width"),
                data.Get("minigame.melt.oscillation"),
                data.Get("minigame.melt.accelerate_factor"),
                data.Get("minigame.melt.accelerate_extra_kwh"));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
