// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Water
{
    public sealed class FixedVein : IIceSource
    {
        private readonly IceBatch _batch;

        public FixedVein(IceBatch batch)
        {
            _batch = batch;
        }

        public static FixedVein FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return new FixedVein(new IceBatch(
                data.Get("minigame.water.vein.liters"),
                data.Get("minigame.water.vein.purity")));
        }

        public IceBatch Take()
        {
            return _batch;
        }
    }
}
