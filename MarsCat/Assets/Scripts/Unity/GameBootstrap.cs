using System;
using System.Collections.Generic;
using MarsCat.Logic.Air;
using MarsCat.Logic.Buildings;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Clock;
using MarsCat.Logic.Data;
using MarsCat.Logic.Energy;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using MarsCat.Logic.Tutorial;
using MarsCat.Logic.Work;
using MarsCat.Unity.UI;
using UnityEngine;

namespace MarsCat.Unity
{
    /// <summary>
    /// Puente con la lógica (#26). Carga el balance de Data/, crea todos los sistemas de MarsCat.Logic,
    /// los avanza cada frame y conecta los eventos entre ellos (trabajadores, energía solar, tutorial).
    /// Hay uno solo en la escena Main; los demás scripts lo reciben por [SerializeField].
    /// Se inicializa en Awake (orden de ejecución adelantado): en Start/OnEnable de los demás ya está listo.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
        // Ids de los edificios del PMV (claves building.<id>.* del CSV) y lo que produce cada uno.
        // No son valores de balance: es la estructura acordada en 06_guia_logica.md.
        public const string SolarPanelsId = "solar_panels";
        public const string MoxieId = "moxie";
        public const string WaterExtractorId = "water_extractor";
        public const string PlantFarmId = "plant_farm";
        public const string RatFarmId = "rat_farm";
        public const string OrganicRecyclerId = "organic_recycler";

        private static readonly string[] BuildingIds =
        {
            SolarPanelsId, MoxieId, WaterExtractorId, PlantFarmId, RatFarmId, OrganicRecyclerId
        };

        private static readonly Dictionary<string, ResourceId> ProducedResources = new Dictionary<string, ResourceId>
        {
            { SolarPanelsId, ResourceId.Electricity },
            { MoxieId, ResourceId.Electricity },        // sin efecto: el O₂ no está en el pool (T3); lo suma el aire (#55)
            { WaterExtractorId, ResourceId.Water },
            { PlantFarmId, ResourceId.Forage },
            { RatFarmId, ResourceId.Food },             // las raciones las produce RatFarm (#57)
            { OrganicRecyclerId, ResourceId.Compost },
        };

        // Paso del tutorial que se completa cuando el edificio empieza a funcionar (02_core_loop.md).
        private static readonly Dictionary<string, TutorialStep> TutorialSteps = new Dictionary<string, TutorialStep>
        {
            { SolarPanelsId, TutorialStep.Energy },
            { MoxieId, TutorialStep.Oxygen },
            { WaterExtractorId, TutorialStep.Water },
            { PlantFarmId, TutorialStep.PlantFarm },
            { RatFarmId, TutorialStep.RatFarm },
            { OrganicRecyclerId, TutorialStep.Recycler },
        };

        private const string SolarProducerId = "solar_panels.output";
        private const string PostJobSuffix = ".post";

        [Tooltip("MarsCat/Assets/Data/balance_pmv.csv")]
        [SerializeField] private TextAsset _balance;

        [Tooltip("MarsCat/Assets/Data/texts_es.csv (clave;texto).")]
        [SerializeField] private TextAsset _texts;

        [Tooltip("Semilla de lo aleatorio (necesidades iniciales de los gatos).")]
        [SerializeField] private int _seed = 1;

        [Tooltip("Edificios cuyo puesto es exterior (un gato degradado no va ahí).")]
        [SerializeField] private string[] _outdoorBuildings = { SolarPanelsId, WaterExtractorId };

        [Tooltip("Saltar la cinemática mientras no exista (#49): el tutorial empieza en 'Mover la cámara'.")]
        [SerializeField] private bool _skipCinematic = true;

        [Tooltip("Empezar en pausa.")]
        [SerializeField] private bool _startPaused;

        [Tooltip("Multiplicador de velocidad para probar (1 = normal).")]
        [Min(0f)]
        [SerializeField] private float _speed = 1f;

        private readonly Dictionary<string, Building> _buildings = new Dictionary<string, Building>(StringComparer.Ordinal);
        private readonly Dictionary<string, Cat> _workers = new Dictionary<string, Cat>(StringComparer.Ordinal);
        private readonly HashSet<string> _postJobs = new HashSet<string>(StringComparer.Ordinal);
        private bool _paused;

        /// <summary>Ya se crearon todos los sistemas.</summary>
        public bool IsReady { get; private set; }

        public BalanceData Balance { get; private set; }
        public TextTable Texts { get; private set; }
        public DayNightClock Clock { get; private set; }
        public ResourcePool Resources { get; private set; }
        public EnergyGrid Energy { get; private set; }
        public CatColony Colony { get; private set; }
        public JobAssigner Jobs { get; private set; }
        public TutorialSequence Tutorial { get; private set; }
        public MoxieFilters MoxieFilters { get; private set; }

        /// <summary>Edificios del PMV por id (solar_panels, moxie, water_extractor, plant_farm, rat_farm, organic_recycler).</summary>
        public IReadOnlyDictionary<string, Building> Buildings => _buildings;

        public bool IsPaused => _paused;

        public float Speed
        {
            get => _speed;
            set => _speed = Mathf.Max(0f, value);
        }

        /// <summary>Se lanza al terminar de crear los sistemas (desde Awake).</summary>
        public event Action Initialized;

        /// <summary>Cambió la pausa (true = en pausa).</summary>
        public event Action<bool> PauseChanged;

        /// <summary>Un gato empezó o dejó de trabajar en un edificio (buildingId, gato o null).</summary>
        public event Action<string, Cat> WorkerChanged;

        /// <summary>Ejecuta la acción ahora si ya está listo, o cuando termine de inicializarse.</summary>
        public void WhenReady(Action action)
        {
            if (action == null) return;
            if (IsReady) action();
            else Initialized += action;
        }

        public Building GetBuilding(string id)
        {
            if (id != null && _buildings.TryGetValue(id, out Building building)) return building;
            return null;
        }

        /// <summary>Gato que trabaja en el puesto del edificio, o null.</summary>
        public Cat GetWorker(string buildingId)
        {
            if (buildingId != null && _workers.TryGetValue(buildingId, out Cat cat)) return cat;
            return null;
        }

        public void SetPaused(bool paused)
        {
            if (_paused == paused) return;
            _paused = paused;
            PauseChanged?.Invoke(paused);
        }

        public void TogglePause() => SetPaused(!_paused);

        /// <summary>Pide la reparación (cobra metal y escombros). false si no alcanza o no está dañado.</summary>
        public bool RequestRepair(string buildingId)
        {
            Building building = GetBuilding(buildingId);
            return building != null && building.StartRepair();
        }

        /// <summary>
        /// Aplica el resultado de un minijuego al edificio. Sin minijuego todavía (lotes siguientes),
        /// el panel puede llamar con PreparationResult.Ok(1f) para probar el ciclo.
        /// </summary>
        public bool ApplyPreparation(string buildingId, PreparationResult result)
        {
            Building building = GetBuilding(buildingId);
            if (building == null) return false;
            if (building.State != BuildingState.Unprepared && building.State != BuildingState.Depleted) return false;
            building.ApplyPreparation(result);
            return true;
        }

        /// <summary>Lo llama la cámara la primera vez que el jugador la mueve (paso 2 del tutorial).</summary>
        public void NotifyCameraMoved()
        {
            if (Tutorial != null && Tutorial.CurrentStep == TutorialStep.MoveCamera)
                Tutorial.Complete(TutorialStep.MoveCamera);
        }

        private void Awake()
        {
            if (_balance == null)
            {
                Debug.LogError("GameBootstrap: falta asignar el TextAsset del balance (Data/balance_pmv.csv).", this);
                enabled = false;
                return;
            }

            Balance = BalanceLoader.Parse(_balance.text);
            Texts = _texts != null ? TextTable.Parse(_texts.text) : TextTable.Empty;
            if (_texts == null)
                Debug.LogWarning("GameBootstrap: falta el TextAsset de textos (Data/texts_es.csv); se mostrarán las claves.", this);

            var clockConfig = DayNightConfig.FromBalance(Balance);
            Clock = new DayNightClock(clockConfig);
            Resources = new ResourcePool(ResourceConfig.FromBalance(Balance));
            Energy = new EnergyGrid(EnergyConfig.FromBalance(Balance), Resources);
            Colony = new CatColony(CatConfig.FromBalance(Balance), Resources, _seed);
            Jobs = new JobAssigner(WorkConfig.FromBalance(Balance));
            Tutorial = new TutorialSequence();
            MoxieFilters = new MoxieFilters(MoxieFilterConfig.FromBalance(Balance));

            float gameSecondsPerSol = clockConfig.DaySeconds + clockConfig.NightSeconds * clockConfig.NightTimeScale;
            foreach (string id in BuildingIds)
                CreateBuilding(id, gameSecondsPerSol);

            // Paneles: producen en la red como fuente solar (solo de día), con el rendimiento de su preparación.
            Energy.RegisterProducer(SolarProducerId, Balance.Get("building." + SolarPanelsId + ".production_rate_per_sol"), true);
            Energy.SetProducerYield(SolarProducerId, 0f);

            Jobs.OnAssigned += HandleAssigned;
            Jobs.OnReleased += HandleReleased;

            Colony.Spawn(Mathf.RoundToInt(Balance.Get("colony.initial_cats")));

            if (_skipCinematic) Tutorial.Complete(TutorialStep.Cinematic);
            _paused = _startPaused;

            IsReady = true;
            Initialized?.Invoke();
            Initialized = null;
        }

        private void CreateBuilding(string id, float gameSecondsPerSol)
        {
            BuildingConfig config = BuildingConfig.FromBalance(Balance, id, ProducedResources[id]);
            if (id == SolarPanelsId || id == MoxieId)
            {
                // Paneles: la producción la lleva EnergyGrid (fuente solar). MOXIE: el O₂ va al aire (#55), no al pool.
                // Se conserva todo el resto del balance; solo se anula la producción hacia el pool.
                config = new BuildingConfig(config.Id, config.RepairMetalCost, config.RepairDebrisCost,
                    config.RepairSeconds, config.ProducedResource, 0f, config.RunSeconds,
                    config.EnergyRatePerSol, config.EnergyPriority, gameSecondsPerSol);
            }

            var building = new Building(config, Resources, Energy);
            building.OnStateChanged += HandleBuildingStateChanged;
            _buildings.Add(id, building);
        }

        private void Update()
        {
            if (!IsReady || _paused) return;

            float dt = Time.deltaTime * _speed;
            if (dt <= 0f) return;

            Clock.Tick(dt);
            float gameDt = Clock.GameDeltaTime;
            bool night = Clock.IsNight;

            Energy.Tick(gameDt, night);
            foreach (Building building in _buildings.Values)
                building.Tick(gameDt);
            if (_buildings[MoxieId].State == BuildingState.Running)
                MoxieFilters.Tick(gameDt);

            Colony.Tick(dt, night);
            Jobs.Tick(dt, Colony, night);
        }

        private void HandleBuildingStateChanged(Building building, BuildingState from, BuildingState to)
        {
            if (building.Id == SolarPanelsId)
                Energy.SetProducerYield(SolarProducerId, to == BuildingState.Running ? building.Yield : 0f);

            // Puesto fijo: existe mientras el edificio funciona. Los paneles no necesitan gato.
            string jobId = building.Id + PostJobSuffix;
            bool wantsPost = to == BuildingState.Running && building.Id != SolarPanelsId;
            if (wantsPost && _postJobs.Add(jobId))
                Jobs.RegisterJob(new Job(jobId, JobKind.Post, building.Id, IsOutdoor(building.Id)));
            else if (!wantsPost && _postJobs.Remove(jobId))
                Jobs.UnregisterJob(jobId);

            if (to == BuildingState.Running && TutorialSteps.TryGetValue(building.Id, out TutorialStep step)
                && Tutorial.CurrentStep == step)
                Tutorial.Complete(step);
        }

        private void HandleAssigned(Job job, Cat cat)
        {
            Building building = GetBuilding(job.BuildingId);
            if (building == null || job.Kind != JobKind.Post) return;
            _workers[job.BuildingId] = cat;
            building.SetWorkerPresent(true);
            WorkerChanged?.Invoke(job.BuildingId, cat);
        }

        private void HandleReleased(Job job, Cat cat)
        {
            Building building = GetBuilding(job.BuildingId);
            if (building == null || job.Kind != JobKind.Post) return;
            _workers.Remove(job.BuildingId);
            building.SetWorkerPresent(false);
            WorkerChanged?.Invoke(job.BuildingId, null);
        }

        private bool IsOutdoor(string buildingId)
        {
            foreach (string id in _outdoorBuildings)
                if (id == buildingId) return true;
            return false;
        }

        private void OnDestroy()
        {
            if (Jobs != null)
            {
                Jobs.OnAssigned -= HandleAssigned;
                Jobs.OnReleased -= HandleReleased;
            }
            foreach (Building building in _buildings.Values)
                building.OnStateChanged -= HandleBuildingStateChanged;
        }
    }
}
