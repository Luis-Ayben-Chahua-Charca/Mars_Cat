// Asistencia de IA: escrito con Codex (OpenAI).
using System;

namespace MarsCat.Logic.Cats
{
    public sealed class Cat
    {
        private readonly CatConfig _config;
        private double _foodSeconds;
        private double _waterSeconds;
        private bool _isNight;

        public int Id { get; }
        public CatState State { get; private set; }
        public float Health { get; private set; }
        public bool IsDegraded { get; }
        public float Hunger => (float)Math.Min(1d, _foodSeconds / _config.FoodDeathSeconds);
        public float Thirst => (float)Math.Min(1d, _waterSeconds / _config.WaterDeathSeconds);

        internal bool FoodAlerted { get; set; }
        internal bool WaterAlerted { get; set; }
        internal event Action<Cat, CatState, CatState> StateChanged;

        public Cat(int id, CatConfig config, float hunger = 0f, float thirst = 0f,
            bool isDegraded = false)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El identificador no puede ser negativo.");
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ValidateInitialNeed(hunger, nameof(hunger));
            ValidateInitialNeed(thirst, nameof(thirst));
            Id = id;
            _foodSeconds = (double)hunger * config.FoodDeathSeconds;
            _waterSeconds = (double)thirst * config.WaterDeathSeconds;
            IsDegraded = isDegraded;
            Health = 1f;
            State = IsExtreme ? CatState.SeekingFood : CatState.Free;
        }

        public bool TryAssign(bool outdoor)
        {
            if (State != CatState.Free || (outdoor && IsDegraded))
                return false;
            SetState(outdoor ? CatState.WorkingOutdoor : CatState.WorkingIndoor);
            return true;
        }

        public void Release()
        {
            if (State == CatState.WorkingIndoor || State == CatState.WorkingOutdoor)
                SetState(IsExtreme ? CatState.SeekingFood : IdleState);
        }

        internal bool IsExtreme => _foodSeconds >= _config.FoodAlertSeconds
            || _waterSeconds >= _config.WaterAlertSeconds;
        internal bool IsFatal => _foodSeconds >= _config.FoodDeathSeconds
            || _waterSeconds >= _config.WaterDeathSeconds;
        private CatState IdleState => _isNight ? CatState.Resting : CatState.Free;

        internal double Elapsed(Need need) => need == Need.Food ? _foodSeconds : _waterSeconds;

        internal void Advance(double seconds)
        {
            _foodSeconds = Math.Min(_config.FoodDeathSeconds, _foodSeconds + seconds);
            _waterSeconds = Math.Min(_config.WaterDeathSeconds, _waterSeconds + seconds);
        }

        internal void Satisfy(Need need)
        {
            if (need == Need.Food)
            {
                _foodSeconds = 0d;
                FoodAlerted = false;
            }
            else
            {
                _waterSeconds = 0d;
                WaterAlerted = false;
            }
        }

        internal void UpdateState(bool isNight)
        {
            _isNight = isNight;
            if (State == CatState.Dead)
                return;
            if (IsExtreme)
                SetState(CatState.SeekingFood);
            else if (State == CatState.Free || State == CatState.Resting || State == CatState.SeekingFood
                || (isNight && (State == CatState.WorkingIndoor || State == CatState.WorkingOutdoor)))
                SetState(IdleState);
        }

        internal void Die()
        {
            Health = 0f;
            SetState(CatState.Dead);
        }

        private void SetState(CatState next)
        {
            if (State == next)
                return;
            CatState previous = State;
            State = next;
            StateChanged?.Invoke(this, previous, next);
        }

        private static void ValidateInitialNeed(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f || value >= 1f)
                throw new ArgumentOutOfRangeException(name, "La necesidad inicial debe ser finita y estar en [0, 1).");
        }
    }
}
