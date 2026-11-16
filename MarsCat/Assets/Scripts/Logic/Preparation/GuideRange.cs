// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;

namespace MarsCat.Logic.Preparation
{
    // Valor guía, mínimo y máximo de un insumo. El rango puede ser asimétrico.
    public sealed class GuideRange
    {
        public const string TooLowSuffix = "too_low";
        public const string TooHighSuffix = "too_high";

        public string StepId { get; }
        public float Guide { get; }
        public float Min { get; }
        public float Max { get; }
        public float Plateau { get; }

        // plateau: fracción de la tolerancia dentro de la cual el puntaje es 1.
        public GuideRange(string stepId, float guide, float min, float max, float plateau)
        {
            if (string.IsNullOrWhiteSpace(stepId))
                throw new ArgumentException("El rango requiere el id del paso.", nameof(stepId));
            if (!IsFinite(guide) || !IsFinite(min) || !IsFinite(max))
                throw new ArgumentOutOfRangeException(nameof(guide), "Guía, mínimo y máximo deben ser números finitos.");
            if (min > guide || guide > max)
                throw new ArgumentException("Debe cumplirse mínimo <= guía <= máximo.", nameof(guide));
            if (!IsFinite(plateau) || plateau < 0f || plateau >= 1f)
                throw new ArgumentOutOfRangeException(nameof(plateau), "La meseta debe estar en [0, 1).");
            StepId = stepId;
            Guide = guide;
            Min = min;
            Max = max;
            Plateau = plateau;
        }

        public StepScore Evaluate(float value)
        {
            if (!IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value), "El valor debe ser un número finito.");
            if (value < Min)
                return StepScore.OutOfRange(ReasonKey(TooLowSuffix));
            if (value > Max)
                return StepScore.OutOfRange(ReasonKey(TooHighSuffix));

            float distance = Math.Abs(value - Guide);
            float tolerance = value < Guide ? Guide - Min : Max - Guide;
            // Con tolerancia 0 de ese lado solo cabe value == Guide (el resto ya salió por el rango).
            if (tolerance <= 0f || distance <= Plateau * tolerance)
                return StepScore.Scored(1f);
            return StepScore.Scored(Math.Max(0f, Math.Min(1f, 1f - distance / tolerance)));
        }

        public string ReasonKey(string suffix)
        {
            return "prep." + StepId + "." + suffix;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
