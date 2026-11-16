// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Data;

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

        public static WorkConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            var groups = new[] { new List<string>(), new List<string>(), new List<string>() };
            const string prefix = "building.";
            const string suffix = ".energy_priority";
            int count = 0;
            // Reutiliza la misma prioridad de BuildingConfig, sin duplicar el balance.
            foreach (string key in data.Keys)
            {
                if (!key.StartsWith(prefix, StringComparison.Ordinal)
                    || !key.EndsWith(suffix, StringComparison.Ordinal))
                    continue;
                if (key.Length <= prefix.Length + suffix.Length)
                    throw new ArgumentException("Identificador de edificio vacío en la clave: " + key);
                string id = key.Substring(prefix.Length, key.Length - prefix.Length - suffix.Length);
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Identificador de edificio vacío en la clave: " + key);
                float priority = data.Get(key);
                if (priority != 0f && priority != 1f && priority != 2f)
                    throw new ArgumentOutOfRangeException(key, "La prioridad debe ser None (0), High (1) o Max (2).");
                groups[(int)priority].Add(id);
                count++;
            }
            if (count == 0)
                throw new KeyNotFoundException("Faltan claves de balance 'building.<id>.energy_priority'.");
            return new WorkConfig(groups[0], groups[1], groups[2]);
        }

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
