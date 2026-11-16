// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Preparation
{
    public sealed class MixConfig
    {
        public float BatchKg { get; }
        public float CompostGuide { get; }
        public float CompostMin { get; }
        public float CompostMax { get; }
        public float BacteriaGuide { get; }
        public float BacteriaMin { get; }
        public float BacteriaMax { get; }
        public float Plateau { get; }

        // Abono y bacterias en % del volumen de la mezcla; el lote en kg de mezcla.
        public MixConfig(float batchKg, float compostGuide, float compostMin, float compostMax,
            float bacteriaGuide, float bacteriaMin, float bacteriaMax, float plateau)
        {
            if (!(batchKg > 0f) || float.IsInfinity(batchKg))
                throw new ArgumentOutOfRangeException(nameof(batchKg), "El lote debe ser positivo y finito.");
            new GuideRange(MixStep.StepId + ".compost", compostGuide, compostMin, compostMax, plateau);
            new GuideRange(MixStep.StepId + ".bacteria", bacteriaGuide, bacteriaMin, bacteriaMax, plateau);
            BatchKg = batchKg;
            CompostGuide = compostGuide;
            CompostMin = compostMin;
            CompostMax = compostMax;
            BacteriaGuide = bacteriaGuide;
            BacteriaMin = bacteriaMin;
            BacteriaMax = bacteriaMax;
            Plateau = plateau;
        }

        public static MixConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return new MixConfig(
                data.Get("minigame.soil.mix.batch_kg"),
                data.Get("minigame.soil.mix.compost.guide"),
                data.Get("minigame.soil.mix.compost.min"),
                data.Get("minigame.soil.mix.compost.max"),
                data.Get("minigame.soil.mix.bacteria.guide"),
                data.Get("minigame.soil.mix.bacteria.min"),
                data.Get("minigame.soil.mix.bacteria.max"),
                data.Get("yield.plateau"));
        }
    }
}
