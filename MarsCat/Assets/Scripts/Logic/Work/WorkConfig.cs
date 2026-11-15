using System;
using System.Collections.Generic;

namespace MarsCat.Logic.Work
{
    public sealed class WorkConfig
    {
        private readonly Dictionary<string, int> _importance = new Dictionary<string, int>(StringComparer.Ordinal);

        // Recibe los mismos grupos None, High y Max de energía, sin depender de su implementación.
        public WorkConfig(IEnumerable<string> noneBuildings, IEnumerable<string> highBuildings,
            IEnumerable<string> maxBuildings)
        {
            Add(noneBuildings, 0, nameof(noneBuildings));
            Add(highBuildings, 1, nameof(highBuildings));
            Add(maxBuildings, 2, nameof(maxBuildings));
        }

        internal int Importance(string buildingId)
        {
            if (!_importance.TryGetValue(buildingId, out int importance))
                throw new KeyNotFoundException("Edificio sin importancia configurada: " + buildingId);
            return importance;
        }

        // FromBalance(BalanceData) requiere el cargador del issue #22 y sus claves acordadas.
        private void Add(IEnumerable<string> buildings, int importance, string name)
        {
            if (buildings == null)
                throw new ArgumentNullException(name);
            foreach (string id in buildings)
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("El identificador de edificio no puede estar vacío.", name);
                if (_importance.ContainsKey(id))
                    throw new ArgumentException("Edificio repetido: " + id, name);
                _importance.Add(id, importance);
            }
        }
    }
}
