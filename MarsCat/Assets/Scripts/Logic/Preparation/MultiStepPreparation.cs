// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;

namespace MarsCat.Logic.Preparation
{
    // Preparación de valor guía y rango: lista de pasos, promedio ponderado y rendimiento común.
    public sealed class MultiStepPreparation
    {
        private readonly List<IPreparationStep> _steps;
        private readonly YieldConfig _config;

        public MultiStepPreparation(IReadOnlyList<IPreparationStep> steps, YieldConfig config)
        {
            if (steps == null)
                throw new ArgumentNullException(nameof(steps));
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (steps.Count == 0)
                throw new ArgumentException("La preparación necesita al menos un paso.", nameof(steps));
            var ids = new HashSet<string>(StringComparer.Ordinal);
            float totalWeight = 0f;
            foreach (IPreparationStep step in steps)
            {
                if (step == null)
                    throw new ArgumentException("Un paso no puede ser nulo.", nameof(steps));
                if (!ids.Add(step.Id))
                    throw new ArgumentException("Id de paso repetido: " + step.Id, nameof(steps));
                totalWeight += config.WeightOf(step.Id);
            }
            if (!(totalWeight > 0f))
                throw new ArgumentException("La suma de los pesos debe ser positiva.", nameof(steps));
            _steps = new List<IPreparationStep>(steps);
            _config = config;
        }

        public IReadOnlyList<IPreparationStep> Steps => _steps;

        // Puntaje S: promedio ponderado de los pasos. Con un paso fuera de rango vale 0.
        public float Score
        {
            get
            {
                float sum = 0f;
                float totalWeight = 0f;
                foreach (IPreparationStep step in _steps)
                {
                    StepScore score = step.Evaluate();
                    if (!score.InRange)
                        return 0f;
                    float weight = _config.WeightOf(step.Id);
                    sum += weight * score.Score;
                    totalWeight += weight;
                }
                return sum / totalWeight;
            }
        }

        // prep.<paso>.improve del paso de menor puntaje cuando S < 1; vacío si todo está en 1 o hay un paso fuera de rango.
        public string ImproveKey
        {
            get
            {
                IPreparationStep worst = null;
                float worstScore = 1f;
                foreach (IPreparationStep step in _steps)
                {
                    StepScore score = step.Evaluate();
                    if (!score.InRange)
                        return string.Empty;
                    if (score.Score < worstScore)
                    {
                        worstScore = score.Score;
                        worst = step;
                    }
                }
                return worst == null ? string.Empty : "prep." + worst.Id + ".improve";
            }
        }

        public float YieldFor(float score)
        {
            if (float.IsNaN(score) || score < 0f || score > 1f)
                throw new ArgumentOutOfRangeException(nameof(score), "El puntaje debe estar entre 0 y 1.");
            return _config.Min + (_config.Max - _config.Min) * score;
        }

        // Fuera de rango o sin recursos: Fail y no cobra nada. Si no, cobra todos los pasos y da Ok(Y).
        public PreparationResult Confirm()
        {
            foreach (IPreparationStep step in _steps)
            {
                StepScore score = step.Evaluate();
                if (!score.InRange)
                    return PreparationResult.Fail(score.ReasonKey);
            }
            foreach (IPreparationStep step in _steps)
            {
                if (!step.CanCharge())
                    return PreparationResult.Fail("prep." + step.Id + ".insufficient");
            }
            float finalScore = Score;
            foreach (IPreparationStep step in _steps)
            {
                if (!step.TryCharge())
                    return PreparationResult.Fail("prep." + step.Id + ".insufficient");
            }
            return PreparationResult.Ok(YieldFor(finalScore));
        }
    }
}
