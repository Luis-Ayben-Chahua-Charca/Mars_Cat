using System;
using MarsCat.Logic.Data;
using MarsCat.Logic.Energy;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Buildings
{
    public sealed class BuildingConfig
    {
        public string Id { get; }
        public float RepairMetalCost { get; }
        public float RepairDebrisCost { get; }
        public float RepairSeconds { get; }
        public ResourceId ProducedResource { get; }
        public float ProductionRatePerSecond { get; }
        public float RunSeconds { get; }
        public float EnergyRatePerSol { get; }
        public EnergyPriority EnergyPriority { get; }

        public BuildingConfig(string id, float repairMetalCost, float repairDebrisCost,
            float repairSeconds, ResourceId producedResource, float productionRatePerSol,
            float runSeconds, float energyRatePerSol, EnergyPriority energyPriority,
            float gameSecondsPerSol)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El edificio necesita un identificador.", nameof(id));
            EnergyConfig.ValidateAmount(repairMetalCost, nameof(repairMetalCost));
            EnergyConfig.ValidateAmount(repairDebrisCost, nameof(repairDebrisCost));
            EnergyConfig.ValidateAmount(repairSeconds, nameof(repairSeconds));
            EnergyConfig.ValidateAmount(runSeconds, nameof(runSeconds));
            if (runSeconds == 0f)
                throw new ArgumentOutOfRangeException(nameof(runSeconds));
            ResourceConfig.ValidateId(producedResource);
            if (!Enum.IsDefined(typeof(EnergyPriority), energyPriority))
                throw new ArgumentOutOfRangeException(nameof(energyPriority));
            var rates = new EnergyConfig(gameSecondsPerSol, 0f);
            ProductionRatePerSecond = rates.ToRatePerSecond(productionRatePerSol);
            EnergyConfig.ValidateAmount(energyRatePerSol, nameof(energyRatePerSol));
            Id = id;
            RepairMetalCost = repairMetalCost;
            RepairDebrisCost = repairDebrisCost;
            RepairSeconds = repairSeconds;
            ProducedResource = producedResource;
            RunSeconds = runSeconds;
            EnergyRatePerSol = energyRatePerSol;
            EnergyPriority = energyPriority;
        }

        public static BuildingConfig FromBalance(BalanceData data, string id, ResourceId producedResource)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El edificio necesita un identificador.", nameof(id));
            var clock = new Clock.DayNightConfig(data.Get("clock.day_seconds"),
                data.Get("clock.night_seconds"), data.Get("clock.night_time_scale"));
            float seconds = (float)((double)clock.DaySeconds +
                (double)clock.NightSeconds * clock.NightTimeScale);
            string prefix = "building." + id + ".";
            float priority = data.Get(prefix + "energy_priority");
            if (priority != (float)EnergyPriority.None && priority != (float)EnergyPriority.High
                && priority != (float)EnergyPriority.Max)
                throw new ArgumentOutOfRangeException(prefix + "energy_priority");
            return new BuildingConfig(id, data.Get(prefix + "repair_metal_cost"),
                data.Get(prefix + "repair_debris_cost"), data.Get(prefix + "repair_seconds"),
                producedResource, data.Get(prefix + "production_rate_per_sol"),
                data.Get(prefix + "run_seconds"), data.Get(prefix + "energy_rate_per_sol"),
                (EnergyPriority)(int)priority, seconds);
        }
    }
}
