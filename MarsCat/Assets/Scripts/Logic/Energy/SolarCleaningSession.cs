// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Energy
{
    public sealed class SolarCleaningSession
    {
        private readonly SolarCleaningConfig _config;
        private readonly ResourcePool _pool;
        private readonly bool[][] _band;
        private readonly int _totalDirty;
        private int _rowIndex;
        private double _rowElapsed;
        private int _cleanedTotal;
        private int _cleanedInRow;

        public int RowsRemaining { get; private set; }
        public bool IsFinished { get; private set; }
        public int TotalDirty => _totalDirty;
        public int CleanedCells => _cleanedTotal;

        // Celdas sucias limpiadas / total de celdas sucias de la banda entera.
        // Si la banda no trae ninguna celda sucia no hay nada que limpiar y la eficiencia es 1.
        public float Efficiency => _totalDirty == 0 ? 1f : (float)_cleanedTotal / _totalDirty;

        // Copia de la fila actual (true = sucia). Al terminar, todas las celdas quedan limpias.
        public bool[] CurrentRow
        {
            get
            {
                var copy = new bool[_config.Lanes];
                if (!IsFinished)
                    Array.Copy(_band[_rowIndex], copy, copy.Length);
                return copy;
            }
        }

        public event Action<int, int> OnRowPassed;
        public event Action<PreparationResult> OnFinished;

        public SolarCleaningSession(SolarCleaningConfig config, ResourcePool pool, int seed)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            var random = new Random(seed);
            _band = new bool[config.Rows][];
            for (int r = 0; r < config.Rows; r++)
            {
                _band[r] = new bool[config.Lanes];
                for (int l = 0; l < config.Lanes; l++)
                {
                    bool dirty = random.NextDouble() < config.DirtyProbability;
                    _band[r][l] = dirty;
                    if (dirty)
                        _totalDirty++;
                }
            }
            RowsRemaining = config.Rows;
        }

        public void Tick(float dt)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0f)
                throw new ArgumentOutOfRangeException(nameof(dt), "El tiempo debe ser finito y no negativo.");
            double remaining = dt;
            while (!IsFinished && remaining > 0d)
            {
                double left = _config.RowSeconds - _rowElapsed;
                if (remaining < left)
                {
                    _rowElapsed += remaining;
                    return;
                }
                remaining -= left;
                PassRow();
            }
        }

        // Devuelve true si el disparo salió (había electricidad). Siempre cobra, acierte o no.
        // Con la sesión terminada no hace nada ni cobra.
        public bool Fire(int lane)
        {
            if (lane < 0 || lane >= _config.Lanes)
                throw new ArgumentOutOfRangeException(nameof(lane), "El carril debe estar entre 0 y " + (_config.Lanes - 1) + ".");
            if (IsFinished)
                return false;
            if (!_pool.TryConsume(ResourceId.Electricity, _config.ShotKwh))
                return false;
            if (_band[_rowIndex][lane])
            {
                _band[_rowIndex][lane] = false;
                _cleanedTotal++;
                _cleanedInRow++;
            }
            return true;
        }

        private void PassRow()
        {
            int missed = 0;
            for (int l = 0; l < _config.Lanes; l++)
                if (_band[_rowIndex][l])
                    missed++;
            int cleaned = _cleanedInRow;
            _cleanedInRow = 0;
            _rowElapsed = 0d;
            _rowIndex++;
            RowsRemaining--;
            if (RowsRemaining == 0)
                IsFinished = true;
            OnRowPassed?.Invoke(cleaned, missed);
            if (IsFinished)
                OnFinished?.Invoke(PreparationResult.Ok(Efficiency));
        }
    }
}
