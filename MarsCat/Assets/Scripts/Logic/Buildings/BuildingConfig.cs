// Asistencia de IA: escrito con Codex (OpenAI); modificado con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            float gameSecondsPerSol, IDictionary<ResourceId, float> byproductRatesPerSol = null)
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

            // Subproductos (ola 3.5): se entregan junto al producto, con el mismo rendimiento y la misma cola de exceso.
            var perSol = new Dictionary<ResourceId, float>();
            var perSecond = new Dictionary<ResourceId, float>();
            if (byproductRatesPerSol != null)
            {
                foreach (KeyValuePair<ResourceId, float> byproduct in byproductRatesPerSol)
                {
                    ResourceConfig.ValidateId(byproduct.Key);
                    EnergyConfig.ValidateAmount(byproduct.Value, nameof(byproductRatesPerSol));
                    if (byproduct.Key == producedResource)
                        throw new ArgumentException("Un subproducto no puede ser el producto principal: " + byproduct.Key,
                            nameof(byproductRatesPerSol));
                    perSol.Add(byproduct.Key, byproduct.Value);
                    perSecond.Add(byproduct.Key, rates.ToRatePerSecond(byproduct.Value));
                }
            }
            ByproductRatesPerSol = new ReadOnlyDictionary<ResourceId, float>(perSol);
            ByproductRatesPerSecond = new ReadOnlyDictionary<ResourceId, float>(perSecond);
        }

        /// <summary>Subproductos por sol (clave building.&lt;id&gt;.byproduct.&lt;recurso&gt;_per_sol). Vacío si no hay.</summary>
        public IReadOnlyDictionary<ResourceId, float> ByproductRatesPerSol { get; }

        /// <summary>Los mismos subproductos, por segundo de juego (T7).</summary>
        public IReadOnlyDictionary<ResourceId, float> ByproductRatesPerSecond { get; }

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

            // Subproductos opcionales: si la clave no está, el edificio no da ese subproducto.
            var byproducts = new Dictionary<ResourceId, float>();
            foreach (ResourceId resource in Enum.GetValues(typeof(ResourceId)))
            {
                if (resource == producedResource || resource == ResourceId.Bacteria || resource == ResourceId.Regolith)
                    continue;
                if (data.TryGet(prefix + "byproduct." + ResourceKey(resource) + "_per_sol", out float rate))
                    byproducts.Add(resource, rate);
            }

            return new BuildingConfig(id, data.Get(prefix + "repair_metal_cost"),
                data.Get(prefix + "repair_debris_cost"), data.Get(prefix + "repair_seconds"),
                producedResource, data.Get(prefix + "production_rate_per_sol"),
                data.Get(prefix + "run_seconds"), data.Get(prefix + "energy_rate_per_sol"),
                (EnergyPriority)(int)priority, seconds, byproducts);
        }

        /// <summary>Nombre del recurso en las claves del balance (cat_waste, crop_residue, forage...).</summary>
        internal static string ResourceKey(ResourceId id)
        {
            switch (id)
            {
                case ResourceId.CatWaste: return "cat_waste";
                case ResourceId.RatWaste: return "rat_waste";
                case ResourceId.CropResidue: return "crop_residue";
                default: return id.ToString().ToLowerInvariant();
            }
        }
    }
}
