// Asistencia de IA: escrito con Codex (OpenAI).
using System;

namespace MarsCat.Logic.Air
{
    public sealed class MoxieFilters
    {
        private readonly MoxieFilterConfig _config;
        private double _dirtyElapsed;
        private double _cleaningElapsed;
        private int _cleaningRemaining;
        private int _cleanedInRequest;
        private bool _workerPresent;
        private bool _ticking;

        public int CleanFilters { get; private set; }

        // Conexión posterior con #55: multiplicar el aporte nominal de O2 por Yield.
        // El consumidor de EnergyGrid permanece activo independientemente de este valor.
        public float Yield => (float)CleanFilters / _config.FilterCount;

        public event Action OnFilterDirty;
        public event Action<int> OnCleaningFinished;

        public MoxieFilters(MoxieFilterConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            CleanFilters = config.FilterCount;
        }

        public void RequestCleaning(int count)
        {
            if (count != 1 && count != 2 && count != 5)
                throw new ArgumentOutOfRangeException(nameof(count), "Se puede solicitar la limpieza de 1, 2 o 5 filtros.");
            if (_cleaningRemaining > 0)
                throw new InvalidOperationException("Ya hay una limpieza pendiente o en curso.");

            // Una solicitud abarca como máximo los filtros sucios al recibirla.
            _cleaningRemaining = Math.Min(count, _config.FilterCount - CleanFilters);
            _cleaningElapsed = 0d;
            _cleanedInRequest = 0;
            if (_cleaningRemaining == 0)
                OnCleaningFinished?.Invoke(0);
        }

        // Unity conecta OnAssigned/OnReleased de #18; perder al gato conserva el progreso.
        public void SetWorkerPresent(bool present)
        {
            _workerPresent = present;
        }

        public void Tick(float gameDt)
        {
            if (float.IsNaN(gameDt) || float.IsInfinity(gameDt) || gameDt < 0f)
                throw new ArgumentOutOfRangeException(nameof(gameDt), "El tiempo debe ser finito y no negativo.");
            if (gameDt == 0f || _ticking)
                return;

            _ticking = true;
            try
            {
                Advance(gameDt);
            }
            finally
            {
                _ticking = false;
            }
        }

        private void Advance(double remaining)
        {
            while (remaining > 0d)
            {
                bool cleaning = _workerPresent && _cleaningRemaining > 0;
                if (CleanFilters == 0 && !cleaning)
                {
                    // No se acumula una deuda de suciedad cuando todos están tapados.
                    _dirtyElapsed = (_dirtyElapsed + remaining) % _config.DirtyIntervalSeconds;
                    return;
                }

                double untilDirty = _config.DirtyIntervalSeconds - _dirtyElapsed;
                double untilClean = cleaning
                    ? _config.CleaningSecondsPerFilter - _cleaningElapsed
                    : double.PositiveInfinity;
                double step = Math.Min(remaining, Math.Min(untilDirty, untilClean));
                bool dirtDue = step == untilDirty;
                bool cleanDue = step == untilClean;
                _dirtyElapsed += step;
                if (cleaning)
                    _cleaningElapsed += step;
                remaining -= step;

                // En un empate se aplica primero la suciedad y luego la limpieza.
                if (dirtDue)
                {
                    _dirtyElapsed = 0d;
                    if (CleanFilters > 0)
                    {
                        CleanFilters--;
                        OnFilterDirty?.Invoke();
                    }
                }

                if (cleanDue)
                {
                    _cleaningElapsed = 0d;
                    CleanFilters++;
                    _cleanedInRequest++;
                    _cleaningRemaining--;
                    if (_cleaningRemaining == 0)
                    {
                        int cleaned = _cleanedInRequest;
                        _cleanedInRequest = 0;
                        // El adaptador retira el Job OneOff para liberar al gato.
                        OnCleaningFinished?.Invoke(cleaned);
                    }
                }
            }
        }
    }
}
