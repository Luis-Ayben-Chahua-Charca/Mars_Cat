// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Water
{
    public sealed class MeltingChamberSession
    {
        private const float Center = 0.5f;
        private const string BoiledKey = "prep.water.boiled";

        private readonly MeltingConfig _config;
        private readonly ResourcePool _pool;
        private double _phase;
        private bool _extraCharged;

        public MeltPhase Phase { get; private set; }
        public float Needle { get; private set; }
        public float Progress { get; private set; }
        public bool IsAccelerated { get; private set; }
        public bool IsFinished => Phase == MeltPhase.Done || Phase == MeltPhase.Lost;

        public event Action<MeltPhase> OnPhaseCompleted;
        public event Action<MeltPhase> OnPhaseFailed;
        public event Action<PreparationResult> OnFinished;

        public MeltingChamberSession(MeltingConfig config, ResourcePool pool, int seed)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            // La fase inicial de la oscilación depende de la semilla (pruebas repetibles).
            _phase = new Random(seed).NextDouble() * 2d * Math.PI;
            Phase = MeltPhase.Pressurize;
            Progress = config.InitialProgress;
            Needle = Clamp01(Center + Oscillation());
        }

        // La electricidad extra se cobra una sola vez por sesión; sin energía no se acelera.
        public void SetAccelerated(bool accelerated)
        {
            if (IsFinished)
                return;
            if (accelerated && !IsAccelerated && !_extraCharged)
            {
                if (!_pool.TryConsume(ResourceId.Electricity, _config.AccelerateExtraKwh))
                    return;
                _extraCharged = true;
            }
            IsAccelerated = accelerated;
        }

        public void Tick(float dt, float input)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0f)
                throw new ArgumentOutOfRangeException(nameof(dt), "El tiempo debe ser finito y no negativo.");
            if (float.IsNaN(input) || input < -1f || input > 1f)
                throw new ArgumentOutOfRangeException(nameof(input), "La entrada debe estar entre -1 y 1.");
            if (IsFinished)
                return;

            // Un ciclo de oscilación dura lo que tarda en llenarse la fase.
            _phase += 2d * Math.PI * dt / _config.FillSeconds;
            Needle = Clamp01(Center + input * Center + Oscillation());

            float speed = IsAccelerated ? _config.AccelerateFactor : 1f;
            bool inZone = Math.Abs(Needle - Center) <= _config.GreenWidth * 0.5f;
            if (inZone)
                Progress += dt * speed * (1f - _config.InitialProgress) / _config.FillSeconds;
            else
                Progress -= dt * _config.DrainPerSecond;

            if (Progress >= 1f)
                Complete();
            else if (Progress <= 0f)
                Fail();
        }

        private float Oscillation()
        {
            float amplitude = _config.Oscillation * (IsAccelerated ? _config.AccelerateFactor : 1f);
            return amplitude * (float)Math.Sin(_phase);
        }

        private void Complete()
        {
            MeltPhase completed = Phase;
            if (completed == MeltPhase.Pressurize)
            {
                Phase = MeltPhase.Heat;
                Progress = _config.InitialProgress;
                OnPhaseCompleted?.Invoke(completed);
                return;
            }

            Progress = 1f;
            Phase = MeltPhase.Done;
            OnPhaseCompleted?.Invoke(completed);
            OnFinished?.Invoke(PreparationResult.Ok(1f));
        }

        private void Fail()
        {
            MeltPhase failed = Phase;
            if (failed == MeltPhase.Pressurize)
            {
                // Fallar la presión solo cuesta tiempo: la fase se reinicia.
                Progress = _config.InitialProgress;
                OnPhaseFailed?.Invoke(failed);
                return;
            }

            Progress = 0f;
            Phase = MeltPhase.Lost;
            OnPhaseFailed?.Invoke(failed);
            OnFinished?.Invoke(PreparationResult.Fail(BoiledKey));
        }

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
