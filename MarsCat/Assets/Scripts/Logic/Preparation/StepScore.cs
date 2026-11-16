// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;

namespace MarsCat.Logic.Preparation
{
    // Resultado de evaluar un paso: si está dentro del rango, su puntaje (0 a 1) y qué corregir si no.
    public readonly struct StepScore
    {
        public bool InRange { get; }
        public float Score { get; }
        public string ReasonKey { get; }

        private StepScore(bool inRange, float score, string reasonKey)
        {
            InRange = inRange;
            Score = score;
            ReasonKey = reasonKey;
        }

        public static StepScore Scored(float score)
        {
            if (float.IsNaN(score) || score < 0f || score > 1f)
                throw new ArgumentOutOfRangeException(nameof(score), "El puntaje debe estar entre 0 y 1.");
            return new StepScore(true, score, string.Empty);
        }

        public static StepScore OutOfRange(string reasonKey)
        {
            if (string.IsNullOrWhiteSpace(reasonKey))
                throw new ArgumentException("Un paso fuera de rango requiere una clave de aviso.", nameof(reasonKey));
            return new StepScore(false, 0f, reasonKey);
        }
    }
}
