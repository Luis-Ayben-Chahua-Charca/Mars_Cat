// Asistencia de IA: escrito con Gemini (Google).
using System;

namespace MarsCat.Logic.Preparation
{
    public readonly struct PreparationResult
    {
        public bool Starts { get; }
        public float Yield { get; }
        public string ReasonKey { get; }

        private PreparationResult(bool starts, float yield, string reasonKey)
        {
            Starts = starts;
            Yield = yield;
            ReasonKey = reasonKey;
        }

        public static PreparationResult Ok(float yield)
        {
            if (float.IsNaN(yield) || float.IsInfinity(yield) || yield < 0f)
                throw new ArgumentOutOfRangeException(nameof(yield),
                    "El rendimiento debe ser finito y no negativo.");
            return new PreparationResult(true, yield, string.Empty);
        }

        public static PreparationResult Fail(string reasonKey)
        {
            if (string.IsNullOrWhiteSpace(reasonKey))
                throw new ArgumentException("El fallo requiere una clave de aviso.", nameof(reasonKey));
            return new PreparationResult(false, 0f, reasonKey);
        }
    }
}
