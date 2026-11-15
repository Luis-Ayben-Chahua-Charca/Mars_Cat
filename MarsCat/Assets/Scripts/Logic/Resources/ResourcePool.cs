using System;
using System.Collections.Generic;

namespace MarsCat.Logic.Resources
{
    public sealed class ResourcePool
    {
        private readonly ResourceConfig _config;
        private readonly Dictionary<ResourceId, float> _amounts;

        public event Action<ResourceId, float> OnChanged;

        public ResourcePool(ResourceConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _config = config;
            _amounts = new Dictionary<ResourceId, float>();
            foreach (ResourceId id in Enum.GetValues(typeof(ResourceId)))
                _amounts.Add(id, config.GetInitialAmount(id));
        }

        public float Get(ResourceId id)
        {
            ResourceConfig.ValidateId(id);
            return _amounts[id];
        }

        public float Capacity(ResourceId id)
        {
            return _config.GetCapacity(id);
        }

        public bool IsInfinite(ResourceId id)
        {
            return _config.IsInfinite(id);
        }

        public float Add(ResourceId id, float amount)
        {
            ResourceConfig.ValidateId(id);
            ResourceConfig.ValidateAmount(amount, nameof(amount));
            if (IsInfinite(id))
                return amount;

            float previous = _amounts[id];
            // La suma en double evita desbordar antes de aplicar el tope.
            float next = (float)Math.Min((double)Capacity(id), (double)previous + amount);
            float added = next - previous;
            if (next != previous)
            {
                _amounts[id] = next;
                OnChanged?.Invoke(id, next);
            }
            return added;
        }

        public bool TryConsume(ResourceId id, float amount)
        {
            ResourceConfig.ValidateId(id);
            ResourceConfig.ValidateAmount(amount, nameof(amount));
            if (IsInfinite(id))
                return true;

            float previous = _amounts[id];
            if (previous < amount)
                return false;

            float next = previous - amount;
            if (next != previous)
            {
                _amounts[id] = next;
                OnChanged?.Invoke(id, next);
            }
            return true;
        }
    }
}
