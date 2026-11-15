using System;
using MarsCat.Logic.Energy;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Buildings
{
    public sealed class Building
    {
        private readonly BuildingConfig _config;
        private readonly ResourcePool _pool;
        private readonly EnergyGrid _energy;
        private double _elapsed;
        private double _pendingProduction;
        private bool _workerPresent;
        private bool _startingRepair;
        private bool _ticking;

        public string Id => _config.Id;
        public BuildingState State { get; private set; } = BuildingState.Damaged;
        public float Yield { get; private set; }
        public event Action<Building, BuildingState, BuildingState> OnStateChanged;

        public Building(BuildingConfig config, ResourcePool pool, EnergyGrid energy)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _energy = energy ?? throw new ArgumentNullException(nameof(energy));
            // El edificio es dueño del registro de su consumidor eléctrico.
            energy.RegisterConsumer(Id, config.EnergyRatePerSol, config.EnergyPriority);
            energy.SetActive(Id, false);
        }

        public bool StartRepair()
        {
            if (State != BuildingState.Damaged || _startingRepair)
                return false;
            if (_pool.Get(ResourceId.Metal) < _config.RepairMetalCost
                || _pool.Get(ResourceId.Debris) < _config.RepairDebrisCost)
                return false;

            _startingRepair = true;
            try
            {
                if (!_pool.TryConsume(ResourceId.Metal, _config.RepairMetalCost))
                    return false;
                if (!_pool.TryConsume(ResourceId.Debris, _config.RepairDebrisCost))
                {
                    _pool.Add(ResourceId.Metal, _config.RepairMetalCost);
                    return false;
                }
                _elapsed = 0d;
                ChangeState(BuildingState.Repairing);
                if (_config.RepairSeconds == 0f)
                    ChangeState(BuildingState.Unprepared);
                return true;
            }
            finally
            {
                _startingRepair = false;
            }
        }

        public void ApplyPreparation(PreparationResult result)
        {
            if (State != BuildingState.Unprepared && State != BuildingState.Depleted)
                throw new InvalidOperationException("El edificio debe estar reparado y esperando preparación.");
            Yield = result.Starts ? result.Yield : 0f;
            _elapsed = 0d;
            ChangeState(result.Starts ? BuildingState.Running : BuildingState.Unprepared);
        }

        public void SetWorkerPresent(bool present)
        {
            _workerPresent = present;
            UpdateEnergyDemand();
        }

        public void Tick(float gameDt)
        {
            EnergyConfig.ValidateAmount(gameDt, nameof(gameDt));
            if (gameDt == 0f || _startingRepair || _ticking)
                return;

            _ticking = true;
            try
            {
                TickCore(gameDt);
            }
            finally
            {
                _ticking = false;
            }
        }

        private void TickCore(float gameDt)
        {
            FlushProduction();
            if (State == BuildingState.Repairing)
            {
                _elapsed += gameDt;
                if (_elapsed >= _config.RepairSeconds)
                {
                    _elapsed = 0d;
                    ChangeState(BuildingState.Unprepared);
                }
                return;
            }
            if (State != BuildingState.Running || !_workerPresent || !_energy.IsPowered(Id))
                return;

            double workingTime = Math.Min(gameDt, _config.RunSeconds - _elapsed);
            _pendingProduction += (double)_config.ProductionRatePerSecond * Yield * workingTime;
            _elapsed += workingTime;
            FlushProduction();
            if (_elapsed >= _config.RunSeconds)
                ChangeState(BuildingState.Depleted);
        }

        private void FlushProduction()
        {
            if (_pendingProduction <= 0d)
                return;
            float amount = (float)Math.Min(float.MaxValue, _pendingProduction);
            // La cola persiste incluso al agotarse o al preparar un nuevo ciclo.
            _pendingProduction = Math.Max(0d, _pendingProduction - _pool.Add(_config.ProducedResource, amount));
        }

        private void ChangeState(BuildingState next)
        {
            if (State == next)
                return;
            BuildingState previous = State;
            State = next;
            UpdateEnergyDemand();
            OnStateChanged?.Invoke(this, previous, next);
        }

        private void UpdateEnergyDemand()
        {
            _energy.SetActive(Id, State == BuildingState.Running && _workerPresent);
        }
    }
}
