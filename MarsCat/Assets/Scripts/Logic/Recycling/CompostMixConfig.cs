using System;
using MarsCat.Logic.Data;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Recycling
{
    // Relación C:N de cada insumo y meta (guía y rango) del Compostador.
    public sealed class CompostMixConfig
    {
        private readonly float[] _cn = new float[3];
        private readonly float[] _nitrogenFraction = new float[3];

        public float Guide { get; }
        public float Min { get; }
        public float Max { get; }
        public float Plateau { get; }

        public CompostMixConfig(float catWasteCn, float catWasteNitrogenFraction,
            float ratWasteCn, float ratWasteNitrogenFraction,
            float cropResidueCn, float cropResidueNitrogenFraction,
            float guide, float min, float max, float plateau)
        {
            Set(ResourceId.CatWaste, catWasteCn, catWasteNitrogenFraction);
            Set(ResourceId.RatWaste, ratWasteCn, ratWasteNitrogenFraction);
            Set(ResourceId.CropResidue, cropResidueCn, cropResidueNitrogenFraction);
            // Valida guía, rango y meseta con las mismas reglas que usa la mezcla.
            new GuideRange(CompostMix.StepId, guide, min, max, plateau);
            Guide = guide;
            Min = min;
            Max = max;
            Plateau = plateau;
        }

        public static bool IsInput(ResourceId id)
        {
            return id == ResourceId.CatWaste || id == ResourceId.RatWaste || id == ResourceId.CropResidue;
        }

        public float CarbonToNitrogen(ResourceId id)
        {
            return _cn[Index(id)];
        }

        public float NitrogenFraction(ResourceId id)
        {
            return _nitrogenFraction[Index(id)];
        }

        public static CompostMixConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            const string prefix = "minigame.compost.mix.";
            return new CompostMixConfig(
                data.Get(prefix + "cat_waste.cn"), data.Get(prefix + "cat_waste.n_fraction"),
                data.Get(prefix + "rat_waste.cn"), data.Get(prefix + "rat_waste.n_fraction"),
                data.Get(prefix + "crop_residue.cn"), data.Get(prefix + "crop_residue.n_fraction"),
                data.Get(prefix + "guide"), data.Get(prefix + "min"), data.Get(prefix + "max"),
                data.Get("yield.plateau"));
        }

        private void Set(ResourceId id, float cn, float nitrogenFraction)
        {
            if (float.IsNaN(cn) || float.IsInfinity(cn) || cn <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cn), "La relación C:N de " + id + " debe ser positiva y finita.");
            if (float.IsNaN(nitrogenFraction) || float.IsInfinity(nitrogenFraction) || nitrogenFraction <= 0f)
                throw new ArgumentOutOfRangeException(nameof(nitrogenFraction), "La fracción de nitrógeno de " + id + " debe ser positiva y finita.");
            _cn[Index(id)] = cn;
            _nitrogenFraction[Index(id)] = nitrogenFraction;
        }

        private static int Index(ResourceId id)
        {
            switch (id)
            {
                case ResourceId.CatWaste: return 0;
                case ResourceId.RatWaste: return 1;
                case ResourceId.CropResidue: return 2;
                default:
                    throw new ArgumentException("El Compostador solo mezcla CatWaste, RatWaste y CropResidue; recibió " + id + ".", nameof(id));
            }
        }
    }
}
