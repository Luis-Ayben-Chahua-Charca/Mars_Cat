// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;

namespace MarsCat.Unity.UI
{
    /// <summary>
    /// Traduce las claves de texto de la lógica (T6: "tutorial.energy.mission", "prep.water.boiled"...) a frases.
    /// Formato del archivo (Data/texts_es.csv): una línea por texto, "clave;texto". La primera línea es el
    /// encabezado; las que empiezan con # son comentarios. El texto puede llevar ';' (se corta solo en el primero)
    /// y "\n" para saltos de línea. Si falta una clave, se devuelve la clave tal cual para que se note.
    /// </summary>
    public sealed class TextTable
    {
        public static readonly TextTable Empty = new TextTable(new Dictionary<string, string>());

        private readonly Dictionary<string, string> _texts;

        private TextTable(Dictionary<string, string> texts)
        {
            _texts = texts;
        }

        public int Count => _texts.Count;

        public static TextTable Parse(string content)
        {
            var texts = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(content)) return new TextTable(texts);

            string[] lines = content.Split('\n');
            for (int i = 1; i < lines.Length; i++)   // la línea 0 es el encabezado
            {
                string line = lines[i].TrimEnd('\r');
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                int separator = line.IndexOf(';');
                if (separator <= 0) continue;
                string key = line.Substring(0, separator).Trim();
                string text = line.Substring(separator + 1).Trim().Replace("\\n", "\n");
                texts[key] = text;
            }
            return new TextTable(texts);
        }

        public bool Has(string key) => key != null && _texts.ContainsKey(key);

        /// <summary>Texto de la clave; si no existe, la propia clave (o vacío si es null).</summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return _texts.TryGetValue(key, out string text) ? text : key;
        }

        /// <summary>Texto con formato: Get(key) con string.Format(args).</summary>
        public string Format(string key, params object[] args)
        {
            string text = Get(key);
            try { return string.Format(text, args); }
            catch (FormatException) { return text; }
        }
    }
}
