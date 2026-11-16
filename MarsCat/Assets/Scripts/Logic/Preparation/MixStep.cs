using System;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Preparation
{
    // Paso 2 de la Fábrica de Suelo: regolito limpio + abono + bacterias.
    public sealed class MixStep : IPreparationStep
    {
        public const string StepId = "mix";

        private readonly MixConfig _config;
        private readonly ResourcePool _pool;
        private readonly GuideRange _compostRange;
        private readonly GuideRange _bacteriaRange;
        private float _compostPercent;
        private float _bacteriaDose;

        public MixStep(MixConfig config, ResourcePool pool)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));
            _config = config;
            _pool = pool;
            _compostRange = new GuideRange(StepId + ".compost", config.CompostGuide, config.CompostMin,
                config.CompostMax, config.Plateau);
            _bacteriaRange = new GuideRange(StepId + ".bacteria", config.BacteriaGuide, config.BacteriaMin,
                config.BacteriaMax, config.Plateau);
        }

        public string Id => StepId;

        public float CompostPercent => _compostPercent;

        public float BacteriaDose => _bacteriaDose;

        // Abono que se cobra: porcentaje del lote de mezcla.
        public float CompostKg => _compostPercent / 100f * _config.BatchKg;

        public void SetCompost(float percent)
        {
            ValidateInput(percent, nameof(percent));
            _compostPercent = percent;
        }

        public void SetBacteria(float dose)
        {
            ValidateInput(dose, nameof(dose));
            _bacteriaDose = dose;
        }

        // Fuera de rango en cualquiera de los dos insumos no arranca; si no, vale el insumo más débil.
        public StepScore Evaluate()
        {
            StepScore compost = _compostRange.Evaluate(_compostPercent);
            if (!compost.InRange)
                return compost;
            StepScore bacteria = _bacteriaRange.Evaluate(_bacteriaDose);
            if (!bacteria.InRange)
                return bacteria;
            return StepScore.Scored(Math.Min(compost.Score, bacteria.Score));
        }

        public bool CanCharge()
        {
            return CompostKg <= _pool.Get(ResourceId.Compost);
        }

        public bool TryCharge()
        {
            return _pool.TryConsume(ResourceId.Compost, CompostKg);
        }

        private static void ValidateInput(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y no negativo.");
        }
    }
}
