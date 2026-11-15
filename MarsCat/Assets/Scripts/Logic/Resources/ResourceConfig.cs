using System;
using System.Collections.Generic;

namespace MarsCat.Logic.Resources
{
    public sealed class ResourceConfig
    {
        private readonly Dictionary<ResourceId, float> _initialAmounts;
        private readonly Dictionary<ResourceId, float> _capacities;

        // Los recursos finitos requieren ambas entradas; los infinitos no necesitan balance.
        public ResourceConfig(IDictionary<ResourceId, float> initialAmounts,
            IDictionary<ResourceId, float> capacities)
        {
            if (initialAmounts == null)
                throw new ArgumentNullException(nameof(initialAmounts));
            if (capacities == null)
                throw new ArgumentNullException(nameof(capacities));

            _initialAmounts = CopyValidated(initialAmounts, nameof(initialAmounts));
            _capacities = CopyValidated(capacities, nameof(capacities));

            foreach (ResourceId id in Enum.GetValues(typeof(ResourceId)))
            {
                if (IsInfinite(id))
                    continue;

                if (!_initialAmounts.ContainsKey(id))
                    throw new ArgumentException("Falta la cantidad inicial de " + id + ".", nameof(initialAmounts));
                if (!_capacities.ContainsKey(id))
                    throw new ArgumentException("Falta la capacidad de " + id + ".", nameof(capacities));
                if (_initialAmounts[id] > _capacities[id])
                    throw new ArgumentException("La cantidad inicial supera la capacidad de " + id + ".", nameof(initialAmounts));
            }
        }

        public float GetInitialAmount(ResourceId id)
        {
            return IsInfinite(id) ? float.PositiveInfinity : _initialAmounts[id];
        }

        public float GetCapacity(ResourceId id)
        {
            return IsInfinite(id) ? float.PositiveInfinity : _capacities[id];
        }

        public bool IsInfinite(ResourceId id)
        {
            ValidateId(id);
            return id == ResourceId.Bacteria || id == ResourceId.Regolith;
        }

        // FromBalance(BalanceData) queda pendiente hasta disponer de la clase de #22.

        internal static void ValidateId(ResourceId id)
        {
            if (!Enum.IsDefined(typeof(ResourceId), id))
                throw new ArgumentOutOfRangeException(nameof(id), id, "El recurso no existe.");
        }

        internal static void ValidateAmount(float amount, string parameterName)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "La cantidad debe ser finita y mayor o igual que cero.");
        }

        private static Dictionary<ResourceId, float> CopyValidated(
            IDictionary<ResourceId, float> values, string parameterName)
        {
            var copy = new Dictionary<ResourceId, float>();
            foreach (var entry in values)
            {
                ValidateId(entry.Key);
                ValidateAmount(entry.Value, parameterName);
                copy.Add(entry.Key, entry.Value);
            }
            return copy;
        }
    }
}
