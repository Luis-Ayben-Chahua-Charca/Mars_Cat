using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Recycling
{
    // Operación Sopa de Bacterias: mantener temperatura y humedad en la zona verde durante el ciclo.
    public sealed class CompostSession
    {
        public const string OverheatedReasonKey = "prep.compost.overheated";

        // Paso máximo de integración (s reales): un Tick grande se parte para no saltarse el sobrecalentamiento.
        private const float MaxSlice = 0.1f;

        // Margen para dar el ciclo por cumplido pese al redondeo de los pasos.
        private const double CycleEpsilon = 1e-4;

        private readonly CompostConfig _config;
        private readonly ResourcePool _pool;
        private float _mixScore;
        private float _heat;
        private bool _sprinkler;
        // Se acumulan en double para que sumar muchos pasos pequeños no deje el ciclo a un epsilon del final.
        private double _elapsed;
        private double _timeInGreen;
        private bool _started;

        public event Action<PreparationResult> OnFinished;

        public CompostSession(CompostConfig config, ResourcePool pool)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));
            _config = config;
            _pool = pool;
            Temperature = config.StartTemp;
            Humidity = config.StartHumidity;
        }

        public float Temperature { get; private set; }
        public float Humidity { get; private set; }
        public float TimeInGreen => (float)_timeInGreen;
        public float HeatLevel => _heat;
        public bool IsSprinklerOn => _sprinkler;
        public bool IsFinished { get; private set; }
        public bool IsRunning => _started && !IsFinished;
        public PreparationResult Result { get; private set; }

        public bool IsTemperatureGreen => Temperature >= _config.TempGreenMin && Temperature <= _config.TempGreenMax;
        public bool IsHumidityGreen => Humidity >= _config.HumidityGreenMin && Humidity <= _config.HumidityGreenMax;
        public bool IsDrowning => Humidity > _config.DrownHumidity;
        public bool IsTooDry => Humidity < _config.DryHumidity;

        // Arranca un ciclo con la mezcla que ya cobró el Compostador (puntaje 0 a 1).
        public void Start(float mixScore)
        {
            if (float.IsNaN(mixScore) || mixScore < 0f || mixScore > 1f)
                throw new ArgumentOutOfRangeException(nameof(mixScore), "El puntaje de la mezcla debe estar entre 0 y 1.");
            if (IsRunning)
                throw new InvalidOperationException("El ciclo ya está en marcha.");
            _mixScore = mixScore;
            _heat = 0f;
            _sprinkler = false;
            _elapsed = 0.0;
            Temperature = _config.StartTemp;
            Humidity = _config.StartHumidity;
            _timeInGreen = 0.0;
            IsFinished = false;
            Result = default;
            _started = true;
        }

        public void SetHeat(float level)
        {
            if (float.IsNaN(level) || level < 0f || level > 1f)
                throw new ArgumentOutOfRangeException(nameof(level), "El nivel de calor debe estar entre 0 y 1.");
            _heat = level;
        }

        public void SetSprinkler(bool on)
        {
            _sprinkler = on;
        }

        public void Tick(float dt)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0f)
                throw new ArgumentOutOfRangeException(nameof(dt), "El tiempo debe ser finito y no negativo.");
            if (!_started)
                throw new InvalidOperationException("Hay que llamar a Start antes de Tick.");

            float remaining = dt;
            while (remaining > 0f && !IsFinished)
            {
                float slice = Math.Min(Math.Min(remaining, MaxSlice), (float)(_config.CycleSeconds - _elapsed));
                Advance(slice);
                remaining -= slice;
            }
        }

        private void Advance(float dt)
        {
            float effectiveHeat = _heat;
            if (effectiveHeat > 0f && !_pool.TryConsume(ResourceId.Electricity, _config.HeatKwhPerSecond * effectiveHeat * dt))
                effectiveHeat = 0f;

            float target = _config.AmbientTemp + effectiveHeat * (_config.MaxHeatTemp - _config.AmbientTemp);
            float previous = Temperature;
            float next = previous + (target - previous) * (1f - (float)Math.Exp(-_config.Inertia * dt));
            // El frío de Marte baja la temperatura de forma constante, sin pasar del ambiente.
            float cooled = next - _config.CoolingPerSecond * dt;
            Temperature = Math.Max(cooled, Math.Min(next, _config.AmbientTemp));

            float humidity = Humidity - _config.DryingPerSecond * dt;
            if (_sprinkler && _pool.TryConsume(ResourceId.Water, _config.SprinklerLitersPerSecond * dt))
                humidity += _config.SprinklerHumidityPerSecond * dt;
            Humidity = Math.Max(0f, Math.Min(100f, humidity));

            _elapsed += dt;
            if (Temperature > _config.OverheatTemp)
            {
                Finish(PreparationResult.Fail(OverheatedReasonKey));
                return;
            }
            if (IsTemperatureGreen && IsHumidityGreen)
                _timeInGreen += dt;
            if (_elapsed >= _config.CycleSeconds - CycleEpsilon)
            {
                float yield = Math.Min(1f, (float)(_timeInGreen / _config.CycleSeconds)) * _mixScore;
                Finish(PreparationResult.Ok(yield));
            }
        }

        private void Finish(PreparationResult result)
        {
            IsFinished = true;
            Result = result;
            OnFinished?.Invoke(result);
        }
    }
}
