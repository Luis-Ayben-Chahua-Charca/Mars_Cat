using System;
using System.Collections.Generic;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Preparation
{
    // Rendimiento de las preparaciones de valor guía: Y = Min + (Max - Min) x S.
    public sealed class YieldConfig
    {
        private readonly Dictionary<string, float> _weights;

        public float Min { get; }
        public float Max { get; }
        public float Plateau { get; }
        public float RobotYield { get; }

        // weights: peso por id de paso; un paso sin entrada pesa 1.
        public YieldConfig(float min, float max, float plateau, float robotYield,
            IDictionary<string, float> weights = null)
        {
            if (IsInvalid(min) || min < 0f)
                throw new ArgumentOutOfRangeException(nameof(min), "El rendimiento mínimo debe ser finito y no negativo.");
            if (IsInvalid(max) || max < min)
                throw new ArgumentOutOfRangeException(nameof(max), "El rendimiento máximo no puede ser menor que el mínimo.");
            if (IsInvalid(plateau) || plateau < 0f || plateau >= 1f)
                throw new ArgumentOutOfRangeException(nameof(plateau), "La meseta debe estar en [0, 1).");
            if (IsInvalid(robotYield) || robotYield < 0f)
                throw new ArgumentOutOfRangeException(nameof(robotYield), "El rendimiento del robot debe ser finito y no negativo.");

            _weights = new Dictionary<string, float>(StringComparer.Ordinal);
            if (weights != null)
            {
                foreach (KeyValuePair<string, float> pair in weights)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                        throw new ArgumentException("El id de un peso no puede estar vacío.", nameof(weights));
                    if (IsInvalid(pair.Value) || pair.Value < 0f)
                        throw new ArgumentOutOfRangeException(nameof(weights), "El peso de " + pair.Key + " debe ser finito y no negativo.");
                    _weights.Add(pair.Key, pair.Value);
                }
            }

            Min = min;
            Max = max;
            Plateau = plateau;
            RobotYield = robotYield;
        }

        public float WeightOf(string stepId)
        {
            if (string.IsNullOrWhiteSpace(stepId))
                throw new ArgumentException("El id del paso no puede estar vacío.", nameof(stepId));
            return _weights.TryGetValue(stepId, out float weight) ? weight : 1f;
        }

        // Lee yield.* y minigame.<game>.<paso>.weight de cada paso indicado.
        public static YieldConfig FromBalance(BalanceData data, string game, IEnumerable<string> stepIds)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(game))
                throw new ArgumentException("Falta el nombre del minijuego.", nameof(game));
            var weights = new Dictionary<string, float>(StringComparer.Ordinal);
            if (stepIds != null)
            {
                foreach (string id in stepIds)
                    weights[id] = data.Get("minigame." + game + "." + id + ".weight");
            }
            return new YieldConfig(data.Get("yield.min"), data.Get("yield.max"),
                data.Get("yield.plateau"), data.Get("yield.robot"), weights);
        }

        private static bool IsInvalid(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value);
        }
    }
}
