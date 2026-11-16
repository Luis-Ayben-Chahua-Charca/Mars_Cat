// Asistencia de IA: escrito con Codex (OpenAI).
using System;

namespace MarsCat.Logic.Clock
{
    public sealed class DayNightClock
    {
        private readonly DayNightConfig _config;
        private readonly double _solGameSeconds;
        private double _phaseElapsedSeconds;

        public int CurrentSol { get; private set; } = 1;
        public bool IsNight { get; private set; }
        public float GameDeltaTime { get; private set; }

        public float DayProgress
        {
            get
            {
                double elapsed = IsNight
                    ? _config.DaySeconds + _phaseElapsedSeconds * _config.NightTimeScale
                    : _phaseElapsedSeconds;
                return (float)(elapsed / _solGameSeconds);
            }
        }

        public event Action<int> OnDayStarted;
        public event Action<int> OnNightStarted;

        public DayNightClock(DayNightConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _solGameSeconds = config.DaySeconds
                + (double)config.NightSeconds * config.NightTimeScale;

            double initialGameSeconds = config.StartProgress * _solGameSeconds;
            IsNight = initialGameSeconds >= config.DaySeconds;
            _phaseElapsedSeconds = IsNight
                ? (initialGameSeconds - config.DaySeconds) / config.NightTimeScale
                : initialGameSeconds;
        }

        public void Tick(float dt)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(dt),
                    "El tiempo real debe ser finito y no negativo.");
            }

            // Se comprueban los límites antes de modificar el estado o emitir eventos.
            double realSolSeconds = (double)_config.DaySeconds + _config.NightSeconds;
            double elapsedInSol = IsNight
                ? _config.DaySeconds + _phaseElapsedSeconds
                : _phaseElapsedSeconds;
            double crossedSols = Math.Floor((elapsedInSol + dt) / realSolSeconds);
            if (crossedSols > int.MaxValue - CurrentSol)
            {
                throw new ArgumentOutOfRangeException(nameof(dt),
                    "El avance excede el rango del contador de soles.");
            }

            double finalRealPosition = (elapsedInSol + dt) % realSolSeconds;
            double finalGamePosition = ToGameSeconds(finalRealPosition);
            double gameDelta = crossedSols * _solGameSeconds
                + finalGamePosition - ToGameSeconds(elapsedInSol);
            if (gameDelta > float.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(dt),
                    "El tiempo de juego del tick excede el rango de float.");
            }

            GameDeltaTime = 0f;
            double remaining = dt;
            double accumulatedGameSeconds = 0d;

            while (remaining > 0d)
            {
                double phaseDuration = IsNight ? _config.NightSeconds : _config.DaySeconds;
                double untilTransition = phaseDuration - _phaseElapsedSeconds;
                bool reachesTransition = remaining >= untilTransition;
                double step = reachesTransition ? untilTransition : remaining;

                accumulatedGameSeconds += step * (IsNight ? _config.NightTimeScale : 1d);
                GameDeltaTime = (float)accumulatedGameSeconds;
                remaining -= step;

                if (!reachesTransition)
                {
                    _phaseElapsedSeconds += step;
                    break;
                }

                _phaseElapsedSeconds = 0d;
                IsNight = !IsNight;
                if (IsNight)
                {
                    OnNightStarted?.Invoke(CurrentSol);
                }
                else
                {
                    CurrentSol++;
                    OnDayStarted?.Invoke(CurrentSol);
                }
            }
        }

        private double ToGameSeconds(double realSeconds)
        {
            return realSeconds <= _config.DaySeconds
                ? realSeconds
                : _config.DaySeconds + (realSeconds - _config.DaySeconds) * _config.NightTimeScale;
        }
    }
}
