// Asistencia de IA: escrito con Codex (OpenAI).
using System;

namespace MarsCat.Logic.Tutorial
{
    public sealed class TutorialSequence
    {
        private bool _isTransitioning;

        public TutorialStep CurrentStep { get; private set; }

        public string MissionKey
        {
            get
            {
                switch (CurrentStep)
                {
                    case TutorialStep.Cinematic: return "tutorial.cinematic.mission";
                    case TutorialStep.MoveCamera: return "tutorial.move_camera.mission";
                    case TutorialStep.Energy: return "tutorial.energy.mission";
                    case TutorialStep.Oxygen: return "tutorial.oxygen.mission";
                    case TutorialStep.Water: return "tutorial.water.mission";
                    case TutorialStep.PlantFarm: return "tutorial.plant_farm.mission";
                    case TutorialStep.RatFarm: return "tutorial.rat_farm.mission";
                    case TutorialStep.Recycler: return "tutorial.recycler.mission";
                    default: return string.Empty;
                }
            }
        }

        public event Action<TutorialStep> OnStepStarted;
        public event Action<TutorialStep> OnStepCompleted;

        public TutorialSequence()
        {
            // El adaptador lee el estado inicial al suscribirse; no se emiten eventos aquí.
            CurrentStep = TutorialStep.Cinematic;
        }

        public bool Complete(TutorialStep step)
        {
            if (!Enum.IsDefined(typeof(TutorialStep), step))
                throw new ArgumentOutOfRangeException("step", "El paso del tutorial no existe.");
            if (_isTransitioning || CurrentStep == TutorialStep.Done || step != CurrentStep)
                return false;

            _isTransitioning = true;
            try
            {
                // El estado y la misión ya están actualizados cuando los observadores los leen.
                CurrentStep = (TutorialStep)((int)step + 1);
                var completed = OnStepCompleted;
                if (completed != null)
                    completed(step);
                var started = OnStepStarted;
                if (started != null)
                    started(CurrentStep);
                return true;
            }
            finally
            {
                // Un observador no puede encadenar avances dentro de la misma notificación.
                _isTransitioning = false;
            }
        }
    }
}
