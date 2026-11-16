// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Recycling
{
    // Paso 1 del reciclador: mezcla de desechos por relación C:N. Es un paso de valor guía y rango.
    public sealed class CompostMix : IPreparationStep
    {
        public const string StepId = "compost";
        public const string EmptyReasonKey = "prep.compost.empty";
        public const string InsufficientReasonKey = "prep.compost.insufficient";

        private static readonly ResourceId[] Inputs =
        {
            ResourceId.CatWaste, ResourceId.RatWaste, ResourceId.CropResidue
        };

        private readonly CompostMixConfig _config;
        private readonly ResourcePool _pool;
        private readonly GuideRange _range;
        private readonly float[] _amounts = new float[3];

        public CompostMix(CompostMixConfig config, ResourcePool pool)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));
            _config = config;
            _pool = pool;
            _range = new GuideRange(StepId, config.Guide, config.Min, config.Max, config.Plateau);
            LastReasonKey = string.Empty;
        }

        public string Id => StepId;

        // Puntaje de la mezcla cobrada en el último TryConfirm exitoso (0 si aún no se confirmó).
        public float ConfirmedScore { get; private set; }

        // Motivo del último TryConfirm fallido; vacío si salió bien.
        public string LastReasonKey { get; private set; }

        // Carbono total / nitrógeno total. 0 si no hay nitrógeno (mezcla vacía).
        public float Ratio
        {
            get
            {
                float carbon = 0f;
                float nitrogen = 0f;
                for (int i = 0; i < Inputs.Length; i++)
                {
                    float n = _amounts[i] * _config.NitrogenFraction(Inputs[i]);
                    nitrogen += n;
                    carbon += n * _config.CarbonToNitrogen(Inputs[i]);
                }
                return nitrogen > 0f ? carbon / nitrogen : 0f;
            }
        }

        public float GetAmount(ResourceId id)
        {
            _config.CarbonToNitrogen(id);
            return _amounts[Array.IndexOf(Inputs, id)];
        }

        public void SetAmount(ResourceId id, float amount)
        {
            // Lanza si el recurso no es un insumo del Compostador.
            _config.CarbonToNitrogen(id);
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "La cantidad debe ser finita y no negativa.");
            _amounts[Array.IndexOf(Inputs, id)] = amount;
        }

        public StepScore Evaluate()
        {
            float total = 0f;
            for (int i = 0; i < _amounts.Length; i++)
                total += _amounts[i];
            if (!(total > 0f))
                return StepScore.OutOfRange(EmptyReasonKey);
            return _range.Evaluate(Ratio);
        }

        public bool CanCharge()
        {
            for (int i = 0; i < Inputs.Length; i++)
            {
                if (_amounts[i] > _pool.Get(Inputs[i]))
                    return false;
            }
            return true;
        }

        public bool TryCharge()
        {
            if (!CanCharge())
                return false;
            for (int i = 0; i < Inputs.Length; i++)
                _pool.TryConsume(Inputs[i], _amounts[i]);
            return true;
        }

        // En rango y con desechos suficientes: cobra y devuelve true. Si no, no cobra nada.
        public bool TryConfirm()
        {
            StepScore score = Evaluate();
            if (!score.InRange)
            {
                LastReasonKey = score.ReasonKey;
                return false;
            }
            if (!TryCharge())
            {
                LastReasonKey = InsufficientReasonKey;
                return false;
            }
            ConfirmedScore = score.Score;
            LastReasonKey = string.Empty;
            return true;
        }
    }
}
