// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Air
{
    public sealed class MoxieFilterConfig
    {
        public int FilterCount { get; }
        public float DirtyIntervalSeconds { get; }
        public float CleaningSecondsPerFilter { get; }

        // Ambos tiempos se expresan en segundos de juego; el balance viene de Data/.
        public MoxieFilterConfig(int filterCount, float dirtyIntervalSeconds, float cleaningSecondsPerFilter)
        {
            if (filterCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(filterCount));
            ValidateDuration(dirtyIntervalSeconds, nameof(dirtyIntervalSeconds));
            ValidateDuration(cleaningSecondsPerFilter, nameof(cleaningSecondsPerFilter));
            FilterCount = filterCount;
            DirtyIntervalSeconds = dirtyIntervalSeconds;
            CleaningSecondsPerFilter = cleaningSecondsPerFilter;
        }

        public static MoxieFilterConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            float count = data.Get("minigame.moxie.filter_count");
            if (count <= 0f || (double)count > int.MaxValue || count != Math.Floor(count))
                throw new ArgumentOutOfRangeException(nameof(data), "La cantidad de filtros debe ser un entero positivo.");
            return new MoxieFilterConfig((int)count,
                data.Get("minigame.moxie.dirty_interval_seconds"),
                data.Get("minigame.moxie.cleaning_seconds_per_filter"));
        }

        private static void ValidateDuration(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name, "La duración debe ser finita y positiva.");
        }
    }
}
