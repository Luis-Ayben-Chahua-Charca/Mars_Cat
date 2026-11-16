// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using System.Linq;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Energy
{
    public sealed class EnergyGrid
    {
        private readonly EnergyConfig _config;
        private readonly ResourcePool _pool;
        private readonly Dictionary<string, float> _producerRates = new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly Dictionary<string, float> _producerYields = new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly HashSet<string> _solar = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, float> _consumerRates = new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly Dictionary<string, EnergyPriority> _priorities = new Dictionary<string, EnergyPriority>(StringComparer.Ordinal);
        private readonly List<string> _consumerOrder = new List<string>();
        private readonly HashSet<string> _active = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _powered = new HashSet<string>(StringComparer.Ordinal);
        private bool _ticking;

        public bool HasPower { get; private set; }
        // Producción solar actual, en unidades de electricidad por segundo de juego.
        public float SolarOutput { get; private set; }
        public event Action OnBlackout;
        public event Action OnPowerRestored;

        public EnergyGrid(EnergyConfig config, ResourcePool pool)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            HasPower = pool.Get(ResourceId.Electricity) > 0f;
        }

        public void RegisterProducer(string id, float ratePerSol, bool solar)
        {
            ValidateNewId(id);
            float rate = _config.ToRatePerSecond(ratePerSol);
            _producerRates.Add(id, rate);
            _producerYields.Add(id, 1f);
            if (solar)
                _solar.Add(id);
        }

        public void SetProducerYield(string id, float yield)
        {
            RequireId(id, _producerRates);
            EnergyConfig.ValidateAmount(yield, nameof(yield));
            _producerYields[id] = yield;
        }

        public void RegisterConsumer(string id, float ratePerSol, EnergyPriority priority)
        {
            ValidateNewId(id);
            if (!Enum.IsDefined(typeof(EnergyPriority), priority))
                throw new ArgumentOutOfRangeException(nameof(priority));
            float rate = _config.ToRatePerSecond(ratePerSol);
            _consumerRates.Add(id, rate);
            _priorities.Add(id, priority);
            _consumerOrder.Add(id);
            _active.Add(id);
        }

        public void SetActive(string id, bool active)
        {
            RequireId(id, _consumerRates);
            if (active)
                _active.Add(id);
            else
            {
                _active.Remove(id);
                _powered.Remove(id);
            }
        }

        public bool IsPowered(string id)
        {
            RequireId(id, _consumerRates);
            return _powered.Contains(id);
        }

        public void Tick(float gameDt, bool isNight)
        {
            EnergyConfig.ValidateAmount(gameDt, nameof(gameDt));
            if (gameDt == 0f || _ticking)
                return;

            _ticking = true;
            try
            {
                TickCore(gameDt, isNight);
            }
            finally
            {
                _ticking = false;
            }
        }

        private void TickCore(float gameDt, bool isNight)
        {
            double generation = 0d;
            double solarOutput = 0d;
            foreach (var producer in _producerRates)
            {
                bool solar = _solar.Contains(producer.Key);
                if (solar && isNight)
                    continue;
                double rate = (double)producer.Value * _producerYields[producer.Key];
                generation += rate;
                if (solar)
                    solarOutput += rate;
            }
            SolarOutput = (float)Math.Min(float.MaxValue, solarOutput);
            // Se aplica el tope al saldo final: la generación abastece al consumo antes de cargar la batería.
            double available = _pool.Get(ResourceId.Electricity) + generation * gameDt;
            double consumed = 0d;
            bool shortage = false;
            _powered.Clear();

            // La calefacción es Max y precede a los consumidores registrados.
            if (isNight)
            {
                double heating = (double)_config.HeatingRatePerSecond * gameDt;
                if (heating <= available)
                    consumed = heating;
                else
                    shortage = true;
            }

            foreach (string id in _consumerOrder.OrderByDescending(id => _priorities[id]))
            {
                if (!_active.Contains(id))
                    continue;
                double demand = (double)_consumerRates[id] * gameDt;
                // Al faltar energía se corta este consumidor y los que siguen en prioridad.
                if (shortage || demand > available - consumed)
                {
                    shortage = true;
                    continue;
                }
                consumed += demand;
                _powered.Add(id);
            }

            float target = (float)Math.Min(_pool.Capacity(ResourceId.Electricity), Math.Max(0d, available - consumed));
            float previous = _pool.Get(ResourceId.Electricity);
            if (target >= previous)
                _pool.Add(ResourceId.Electricity, target - previous);
            else
                _pool.TryConsume(ResourceId.Electricity, previous - target);

            bool hasPower = !shortage && (available > 0d || _active.Count > 0);
            if (hasPower == HasPower)
                return;
            HasPower = hasPower;
            if (hasPower)
                OnPowerRestored?.Invoke();
            else
                OnBlackout?.Invoke();
        }

        private void ValidateNewId(string id)
        {
            ValidateId(id);
            if (_producerRates.ContainsKey(id) || _consumerRates.ContainsKey(id))
                throw new ArgumentException("Identificador de energía repetido: " + id, nameof(id));
        }

        private static void RequireId(string id, Dictionary<string, float> entries)
        {
            ValidateId(id);
            if (!entries.ContainsKey(id))
                throw new KeyNotFoundException("Identificador de energía desconocido: " + id);
        }

        private static void ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El identificador no puede estar vacío.", nameof(id));
        }
    }
}
