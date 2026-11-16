// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Food
{
    public sealed class RatHunt
    {
        private readonly CatColony _colony;
        private readonly LooseRats _loose;
        private readonly ResourcePool _pool;
        private readonly RatHuntConfig _config;
        private readonly Dictionary<int, double> _huntElapsed = new Dictionary<int, double>();
        private bool _noLooseNotified;

        public event Action<Cat, int> OnHunted;
        public event Action OnNoLooseRats;

        public RatHunt(CatColony colony, LooseRats loose, ResourcePool pool, RatHuntConfig config)
        {
            _colony = colony ?? throw new ArgumentNullException(nameof(colony));
            _loose = loose ?? throw new ArgumentNullException(nameof(loose));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // Se llama después de CatColony.Tick, con los mismos segundos reales (el hambre usa tiempo real).
        public void Tick(float dt)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0f)
                throw new ArgumentOutOfRangeException(nameof(dt), "El tiempo debe ser finito y no negativo.");

            bool noRations = _pool.Get(ResourceId.Food) < _config.FoodRation;
            foreach (Cat cat in _colony.Cats)
            {
                if (!noRations || cat.State != CatState.SeekingFood)
                {
                    _huntElapsed.Remove(cat.Id);
                    continue;
                }

                _huntElapsed.TryGetValue(cat.Id, out double elapsed);
                elapsed += dt;
                if (elapsed < _config.HuntSeconds)
                {
                    _huntElapsed[cat.Id] = elapsed;
                    continue;
                }

                int ratId = _loose.AnyLoose();
                if (ratId < 0)
                {
                    // El gato queda listo para cazar; el aviso sale una sola vez.
                    _huntElapsed[cat.Id] = _config.HuntSeconds;
                    if (!_noLooseNotified)
                    {
                        _noLooseNotified = true;
                        OnNoLooseRats?.Invoke();
                    }
                    continue;
                }

                _huntElapsed.Remove(cat.Id);
                _loose.TryTake(ratId);
                cat.Satisfy(Need.Food);
                OnHunted?.Invoke(cat, ratId);
            }
        }
    }
}
