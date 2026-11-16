// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;

namespace MarsCat.Logic.Water
{
    public readonly struct IceBatch
    {
        public float Liters { get; }
        public float Purity { get; }

        public IceBatch(float liters, float purity)
        {
            if (float.IsNaN(liters) || float.IsInfinity(liters) || liters < 0f)
                throw new ArgumentOutOfRangeException(nameof(liters), "Los litros deben ser finitos y no negativos.");
            if (float.IsNaN(purity) || purity < 0f || purity > 1f)
                throw new ArgumentOutOfRangeException(nameof(purity), "La pureza debe estar entre 0 y 1.");
            Liters = liters;
            Purity = purity;
        }
    }
}
