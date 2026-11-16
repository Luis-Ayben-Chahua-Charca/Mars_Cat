using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Preparation
{
    public sealed class WashConfig
    {
        public float BatchKg { get; }
        public float InitialPercent { get; }
        public float ThresholdPercent { get; }
        public float GuideLiters { get; }
        public float MinLiters { get; }
        public float MaxLiters { get; }
        public float Plateau { get; }

        public WashConfig(float batchKg, float initialPercent, float thresholdPercent,
            float guideLiters, float minLiters, float maxLiters, float plateau)
        {
            if (!(batchKg > 0f) || float.IsInfinity(batchKg))
                throw new ArgumentOutOfRangeException(nameof(batchKg), "El lote debe ser positivo y finito.");
            if (!(initialPercent >= 0f) || float.IsInfinity(initialPercent))
                throw new ArgumentOutOfRangeException(nameof(initialPercent), "El perclorato inicial debe ser finito y no negativo.");
            if (!(thresholdPercent >= 0f) || float.IsInfinity(thresholdPercent))
                throw new ArgumentOutOfRangeException(nameof(thresholdPercent), "El umbral debe ser finito y no negativo.");
            // Valida guía, rango y meseta con las mismas reglas que usa el paso.
            new GuideRange(WashStep.StepId, guideLiters, minLiters, maxLiters, plateau);
            BatchKg = batchKg;
            InitialPercent = initialPercent;
            ThresholdPercent = thresholdPercent;
            GuideLiters = guideLiters;
            MinLiters = minLiters;
            MaxLiters = maxLiters;
            Plateau = plateau;
        }

        public static WashConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return new WashConfig(
                data.Get("minigame.soil.wash.batch_kg"),
                data.Get("minigame.soil.wash.initial_percent"),
                data.Get("minigame.soil.wash.threshold_percent"),
                data.Get("minigame.soil.wash.guide"),
                data.Get("minigame.soil.wash.min"),
                data.Get("minigame.soil.wash.max"),
                data.Get("yield.plateau"));
        }
    }
}
