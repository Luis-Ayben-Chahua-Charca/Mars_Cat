using System;
using MarsCat.Logic.Data;

namespace MarsCat.Logic.Food
{
    public sealed class LooseRats
    {
        private readonly bool[] _loose;

        public int Count { get; private set; }

        public event Action<int> OnTaken;

        public LooseRats(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "La cantidad de ratas sueltas no puede ser negativa.");
            _loose = new bool[count];
            for (int i = 0; i < count; i++)
                _loose[i] = true;
            Count = count;
        }

        public static LooseRats FromBalance(BalanceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return new LooseRats(RatFarmConfig.ReadInt(data, "rat.loose.initial"));
        }

        public bool IsLoose(int id)
        {
            return id >= 0 && id < _loose.Length && _loose[id];
        }

        public bool TryTake(int id)
        {
            if (!IsLoose(id))
                return false;
            _loose[id] = false;
            Count--;
            OnTaken?.Invoke(id);
            return true;
        }

        public int AnyLoose()
        {
            for (int i = 0; i < _loose.Length; i++)
                if (_loose[i])
                    return i;
            return -1;
        }
    }
}
