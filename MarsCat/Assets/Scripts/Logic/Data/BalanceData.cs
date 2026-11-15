using System;
using System.Collections.Generic;

namespace MarsCat.Logic.Data
{
    public sealed class BalanceData
    {
        private readonly Dictionary<string, float> _values;
        private readonly Dictionary<string, string> _sources;

        internal BalanceData(Dictionary<string, float> values, Dictionary<string, string> sources)
        {
            _values = new Dictionary<string, float>(values, StringComparer.Ordinal);
            _sources = new Dictionary<string, string>(sources, StringComparer.Ordinal);
        }

        public float Get(string key)
        {
            ValidateKey(key);
            if (!_values.TryGetValue(key, out float value))
                throw new KeyNotFoundException($"Falta la clave de balance '{key}'.");
            return value;
        }

        public bool TryGet(string key, out float value)
        {
            ValidateKey(key);
            return _values.TryGetValue(key, out value);
        }

        public string Source(string key)
        {
            ValidateKey(key);
            if (!_sources.TryGetValue(key, out string source))
                throw new KeyNotFoundException($"Falta la clave de balance '{key}'.");
            return source;
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("La clave de balance no puede estar vacía.", nameof(key));
        }
    }
}
