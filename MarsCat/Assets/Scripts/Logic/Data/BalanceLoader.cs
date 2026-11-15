using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MarsCat.Logic.Data
{
    public static class BalanceLoader
    {
        public static BalanceData Parse(string csv)
        {
            if (csv == null)
                throw new ArgumentNullException(nameof(csv));

            var values = new Dictionary<string, float>(StringComparer.Ordinal);
            var sources = new Dictionary<string, string>(StringComparer.Ordinal);
            using (var reader = new StringReader(csv))
            {
                string header = reader.ReadLine();
                if (header == null || header.TrimStart('\uFEFF').Trim() != "clave;valor;unidad;fuente")
                    throw InvalidLine(1, "Se esperaba el encabezado clave;valor;unidad;fuente.");

                int lineNumber = 1;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                        continue;

                    string[] columns = line.Split(';');
                    if (columns.Length != 4)
                        throw InvalidLine(lineNumber, "Se esperaban cuatro columnas separadas por punto y coma.");

                    string key = columns[0].Trim();
                    string source = columns[3].Trim();
                    if (key.Length == 0 || source.Length == 0)
                        throw InvalidLine(lineNumber, "La clave y la fuente son obligatorias.");
                    if (values.ContainsKey(key))
                        throw InvalidLine(lineNumber, $"La clave '{key}' está duplicada.");
                    if (!float.TryParse(columns[1].Trim(), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out float value)
                        || float.IsNaN(value) || float.IsInfinity(value))
                        throw InvalidLine(lineNumber, $"El valor de '{key}' debe ser un número finito con punto decimal.");

                    values.Add(key, value);
                    sources.Add(key, source);
                }
            }
            return new BalanceData(values, sources);
        }

        private static FormatException InvalidLine(int lineNumber, string message)
        {
            return new FormatException($"Línea {lineNumber}: {message}");
        }
    }
}
