// Asistencia de IA: escrito con Codex (OpenAI); modificado con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using System.Linq;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Cats
{
    public sealed class CatColony
    {
        private readonly CatConfig _config;
        private readonly ResourcePool _pool;
        private readonly Random _random;
        private readonly List<Cat> _cats = new List<Cat>();
        private bool _isNight;

        public IReadOnlyList<Cat> Cats { get; }
        public IEnumerable<Cat> FreeCats => _cats.Where(cat => cat.State == CatState.Free);
        public event Action<Cat, CatState, CatState> OnStateChanged;
        public event Action<Cat, Need> OnExtremeAlert;
        public event Action<Cat> OnDied;

        public CatColony(CatConfig config, ResourcePool pool, int seed)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _random = new Random(seed);
            Cats = _cats.AsReadOnly();
        }

        public void Spawn(int count)
        {
            if (count < 0 || count > int.MaxValue - _cats.Count)
                throw new ArgumentOutOfRangeException(nameof(count), "La cantidad de gatos es inválida.");
            for (int i = 0; i < count; i++)
            {
                var cat = new Cat(_cats.Count, _config,
                    (float)(_random.NextDouble() * _config.InitialNeedSpread),
                    (float)(_random.NextDouble() * _config.InitialNeedSpread));
                _cats.Add(cat);
                cat.StateChanged += ForwardStateChanged;
                cat.UpdateState(_isNight);
            }
        }

        public void Tick(float dt, bool isNight)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0f)
                throw new ArgumentOutOfRangeException(nameof(dt), "El tiempo debe ser finito y no negativo.");
            _isNight = isNight;
            double remaining = dt;
            // Avanzar por umbrales conserva comidas y alertas incluso con ticks grandes.
            while (true)
            {
                ProcessNeeds();
                if (remaining <= 0d)
                    return;
                double step = remaining;
                bool alive = false;
                foreach (Cat cat in _cats)
                {
                    if (cat.State == CatState.Dead)
                        continue;
                    alive = true;
                    step = Math.Min(step, NextBoundary(cat.Elapsed(Need.Food),
                        _config.FoodIntervalSeconds, _config.FoodAlertSeconds, _config.FoodDeathSeconds));
                    step = Math.Min(step, NextBoundary(cat.Elapsed(Need.Water),
                        _config.WaterIntervalSeconds, _config.WaterAlertSeconds, _config.WaterDeathSeconds));
                }
                if (!alive)
                    return;
                foreach (Cat cat in _cats)
                    if (cat.State != CatState.Dead)
                        cat.Advance(step);
                remaining -= step;
            }
        }

        private void ProcessNeeds()
        {
            foreach (Cat cat in _cats)
            {
                if (cat.State != CatState.Dead && cat.IsFatal)
                {
                    cat.Die();
                    OnDied?.Invoke(cat);
                }
            }
            Feed(Need.Food, ResourceId.Food, _config.FoodIntervalSeconds, _config.FoodRation);
            Feed(Need.Water, ResourceId.Water, _config.WaterIntervalSeconds, _config.WaterRation);
            foreach (Cat cat in _cats)
            {
                if (cat.State == CatState.Dead)
                    continue;
                cat.UpdateState(_isNight);
                if (!cat.FoodAlerted && cat.Elapsed(Need.Food) >= _config.FoodAlertSeconds)
                {
                    cat.FoodAlerted = true;
                    OnExtremeAlert?.Invoke(cat, Need.Food);
                }
                if (!cat.WaterAlerted && cat.Elapsed(Need.Water) >= _config.WaterAlertSeconds)
                {
                    cat.WaterAlerted = true;
                    OnExtremeAlert?.Invoke(cat, Need.Water);
                }
            }
        }

        private void Feed(Need need, ResourceId resource, float interval, float ration)
        {
            // Empates por identificador para mantener resultados reproducibles.
            Cat[] candidates = _cats.Where(cat => cat.State != CatState.Dead && cat.Elapsed(need) >= interval)
                .OrderByDescending(cat => cat.Elapsed(need)).ThenBy(cat => cat.Id).ToArray();
            foreach (Cat cat in candidates)
            {
                if (!_pool.TryConsume(resource, ration))
                    break;
                cat.Satisfy(need);
                if (need == Need.Food && _config.WastePerMeal > 0f)
                    _pool.Add(ResourceId.CatWaste, _config.WastePerMeal);
            }
        }

        private static double NextBoundary(double elapsed, double interval, double alert, double death)
        {
            if (elapsed < interval)
                return interval - elapsed;
            if (elapsed < alert)
                return alert - elapsed;
            return death - elapsed;
        }

        private void ForwardStateChanged(Cat cat, CatState from, CatState to)
        {
            OnStateChanged?.Invoke(cat, from, to);
        }
    }
}
