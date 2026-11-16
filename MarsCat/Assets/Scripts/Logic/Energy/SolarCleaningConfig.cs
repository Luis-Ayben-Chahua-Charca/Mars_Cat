using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Energy
{
    public sealed class SolarCleaningConfig
    {
        public int Lanes { get; }
        public float RowSeconds { get; }
        public int Rows { get; }
        public float DirtyProbability { get; }
        public float ShotKwh { get; }

        // RowSeconds va en segundos reales; ShotKwh es la electricidad que cobra cada disparo.
        public SolarCleaningConfig(int lanes, float rowSeconds, int rows, float dirtyProbability, float shotKwh)
        {
            if (lanes <= 0)
                throw new ArgumentOutOfRangeException(nameof(lanes), "Debe haber al menos un carril.");
            if (float.IsNaN(rowSeconds) || float.IsInfinity(rowSeconds) || rowSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(rowSeconds), "La duración de la fila debe ser finita y positiva.");
            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows), "Debe haber al menos una fila.");
            if (float.IsNaN(dirtyProbability) || dirtyProbability < 0f || dirtyProbability > 1f)
                throw new ArgumentOutOfRangeException(nameof(dirtyProbability), "La probabilidad debe estar entre 0 y 1.");
            if (float.IsNaN(shotKwh) || float.IsInfinity(shotKwh) || shotKwh < 0f)
                throw new ArgumentOutOfRangeException(nameof(shotKwh), "El costo del disparo debe ser finito y no negativo.");
            Lanes = lanes;
            RowSeconds = rowSeconds;
            Rows = rows;
            DirtyProbability = dirtyProbability;
            ShotKwh = shotKwh;
        }

        public static SolarCleaningConfig FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return new SolarCleaningConfig(
                ReadInt(data, "minigame.solar.lanes"),
                data.Get("minigame.solar.row_seconds"),
                ReadInt(data, "minigame.solar.rows"),
                data.Get("minigame.solar.dirty_probability"),
                data.Get("minigame.solar.shot_kwh"));
        }

        private static int ReadInt(BalanceData data, string key)
        {
            float value = data.Get(key);
            if (float.IsNaN(value) || value <= 0f || value > int.MaxValue || value != Math.Floor(value))
                throw new ArgumentOutOfRangeException(nameof(data), "La clave " + key + " debe ser un entero positivo.");
            return (int)value;
        }
    }
}
