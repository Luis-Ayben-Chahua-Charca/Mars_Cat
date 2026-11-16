using System;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Preparation
{
    // Paso 1 de la Fábrica de Suelo: lavar el regolito para bajar el perclorato.
    public sealed class WashStep : IPreparationStep
    {
        public const string StepId = "wash";

        // Tolerancia relativa para no fallar por redondeo cuando el perclorato queda justo en el umbral.
        private const float ThresholdSlack = 1e-4f;

        private readonly WashConfig _config;
        private readonly ResourcePool _pool;
        private readonly GuideRange _range;
        private float _liters;

        public WashStep(WashConfig config, ResourcePool pool)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));
            _config = config;
            _pool = pool;
            _range = new GuideRange(StepId, config.GuideLiters, config.MinLiters, config.MaxLiters, config.Plateau);
        }

        public string Id => StepId;

        public float Liters => _liters;

        // Perclorato restante (% de la masa): inicial / (1 + litros / kg del lote).
        public float PerchloratePercent => _config.InitialPercent / (1f + _liters / _config.BatchKg);

        public void SetWater(float liters)
        {
            if (float.IsNaN(liters) || float.IsInfinity(liters) || liters < 0f)
                throw new ArgumentOutOfRangeException(nameof(liters), "Los litros deben ser finitos y no negativos.");
            _liters = liters;
        }

        public StepScore Evaluate()
        {
            StepScore score = _range.Evaluate(_liters);
            if (!score.InRange)
                return score;
            if (PerchloratePercent > _config.ThresholdPercent * (1f + ThresholdSlack))
                return StepScore.OutOfRange(_range.ReasonKey(GuideRange.TooLowSuffix));
            return score;
        }

        public bool CanCharge()
        {
            return _liters <= _pool.Get(ResourceId.Water);
        }

        public bool TryCharge()
        {
            return _pool.TryConsume(ResourceId.Water, _liters);
        }
    }
}
