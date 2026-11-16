// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Food
{
    public sealed class RatFarm
    {
        private const double Epsilon = 1e-9;

        private readonly RatFarmConfig _config;
        private readonly ResourcePool _pool;
        private double _breedProgress;
        private double _convertElapsed;
        private bool _workerPresent;

        public int Rats { get; private set; }
        public bool CanStart => Rats > _config.MinBreeders;

        public event Action<int> OnRatsChanged;
        public event Action<float> OnRationsProduced;

        public RatFarm(RatFarmConfig config, ResourcePool pool)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        // Sin gato en el puesto no se convierte; el progreso de la rata en curso se conserva.
        public void SetWorkerPresent(bool present)
        {
            _workerPresent = present;
        }

        public int AddRats(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "La cantidad de ratas no puede ser negativa.");
            int added = Math.Min(count, _config.MaxRats - Rats);
            if (added > 0)
            {
                Rats += added;
                OnRatsChanged?.Invoke(Rats);
            }
            return added;
        }

        public int RemoveRats(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "La cantidad de ratas no puede ser negativa.");
            int removed = Math.Min(count, Rats);
            if (removed > 0)
            {
                Rats -= removed;
                OnRatsChanged?.Invoke(Rats);
            }
            return removed;
        }

        public void Tick(float gameDt)
        {
            if (float.IsNaN(gameDt) || float.IsInfinity(gameDt) || gameDt < 0f)
                throw new ArgumentOutOfRangeException(nameof(gameDt), "El tiempo debe ser finito y no negativo.");

            double remaining = gameDt;
            while (remaining > 0d && Rats > 0)
            {
                double birthRate = (double)_config.BreedRatePerSecond * Rats * (1d - (double)Rats / _config.MaxRats);
                bool converting = _workerPresent && CanStart && HasRoomForRations();
                double untilBirth = birthRate > 0d
                    ? (1d - _breedProgress) / birthRate : double.PositiveInfinity;
                double untilConvert = converting
                    ? _config.ConvertSeconds - _convertElapsed : double.PositiveInfinity;
                double step = Math.Min(remaining, Math.Min(untilBirth, untilConvert));
                remaining -= step;

                // Sin forraje la granja se detiene por completo; con poco, avanza proporcionalmente.
                double fed = Feed(step);
                if (fed <= 0d)
                    return;
                double effective = step * fed;
                _breedProgress += birthRate * effective;
                if (converting)
                    _convertElapsed += effective;
                ProduceWaste(effective);

                if (birthRate > 0d && _breedProgress >= 1d - Epsilon)
                {
                    _breedProgress = Math.Max(0d, _breedProgress - 1d);
                    if (Rats < _config.MaxRats)
                    {
                        Rats++;
                        OnRatsChanged?.Invoke(Rats);
                    }
                }
                if (converting && _convertElapsed >= _config.ConvertSeconds - Epsilon)
                    Convert();
            }
        }

        private bool HasRoomForRations()
        {
            return _pool.IsInfinite(ResourceId.Food)
                || _pool.Capacity(ResourceId.Food) - _pool.Get(ResourceId.Food) > 0f;
        }

        // Devuelve la fracción [0, 1] del paso en que las ratas tuvieron forraje.
        private double Feed(double step)
        {
            double need = (double)_config.ForagePerRatPerSecond * Rats * step;
            if (need <= 0d)
                return 1d;
            float available = _pool.Get(ResourceId.Forage);
            float consumed = Math.Min(available, (float)need);
            if (consumed > 0f)
                _pool.TryConsume(ResourceId.Forage, consumed);
            // El redondeo a float no debe contar como falta de forraje.
            double fed = consumed / need;
            return fed > 1d - 1e-6 ? 1d : fed;
        }

        private void ProduceWaste(double effective)
        {
            float waste = (float)((double)_config.WastePerRatPerSecond * Rats * effective);
            if (waste > 0f)
                _pool.Add(ResourceId.RatWaste, waste);
        }

        private void Convert()
        {
            _convertElapsed = 0d;
            Rats--;
            OnRatsChanged?.Invoke(Rats);
            float produced = _pool.Add(ResourceId.Food, _config.RationsPerRat);
            OnRationsProduced?.Invoke(produced);
        }
    }
}
