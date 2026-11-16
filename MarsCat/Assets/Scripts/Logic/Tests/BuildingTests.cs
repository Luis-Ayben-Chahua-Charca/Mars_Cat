// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Buildings;
using MarsCat.Logic.Energy;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class BuildingTests
    {
        private ResourcePool _pool;
        private EnergyGrid _grid;
        private Building _building;

        [SetUp]
        public void SetUp()
        {
            _pool = new ResourcePool(new ResourceConfig(ResourceConfigTests.FiniteValues(10f),
                ResourceConfigTests.FiniteValues(100f)));
            _grid = new EnergyGrid(new EnergyConfig(10f, 0f), _pool);
            _building = new Building(BuildingConfigTests.Config(), _pool, _grid);
        }

        private void Prepare(float yield = 1f)
        {
            Assert.That(_building.StartRepair(), Is.True);
            _building.Tick(2f);
            _building.ApplyPreparation(PreparationResult.Ok(yield));
        }

        [Test]
        public void Repair_ChargesBothMaterialsOnceAndCompletesWithoutWorker()
        {
            Assert.That(_building.State, Is.EqualTo(BuildingState.Damaged));
            Assert.That(_building.StartRepair(), Is.True);
            Assert.That(_pool.Get(ResourceId.Metal), Is.EqualTo(8f));
            Assert.That(_pool.Get(ResourceId.Debris), Is.EqualTo(7f));
            Assert.That(_building.StartRepair(), Is.False);
            _building.Tick(1f);
            Assert.That(_building.State, Is.EqualTo(BuildingState.Repairing));
            _building.Tick(100f);
            Assert.That(_building.State, Is.EqualTo(BuildingState.Unprepared));
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(10f));
            Assert.That(_pool.Get(ResourceId.Metal), Is.EqualTo(8f));
        }

        [TestCase(ResourceId.Metal)]
        [TestCase(ResourceId.Debris)]
        public void Repair_InsufficientEitherMaterialDoesNotChargeOther(ResourceId missing)
        {
            _pool.TryConsume(missing, 10f);
            Assert.That(_building.StartRepair(), Is.False);
            Assert.That(_building.State, Is.EqualTo(BuildingState.Damaged));
            var other = missing == ResourceId.Metal ? ResourceId.Debris : ResourceId.Metal;
            Assert.That(_pool.Get(other), Is.EqualTo(10f));
        }

        [Test]
        public void Repair_RejectsReentrantStartFromResourceEvent()
        {
            _pool.OnChanged += (id, amount) =>
            {
                if (id == ResourceId.Metal)
                    Assert.That(_building.StartRepair(), Is.False);
            };
            Assert.That(_building.StartRepair(), Is.True);
            Assert.That(_pool.Get(ResourceId.Metal), Is.EqualTo(8f));
        }

        [Test]
        public void FreeInstantRepair_ReachesUnpreparedAndEmitsBothTransitions()
        {
            var building = new Building(BuildingConfigTests.Config(metal: 0f, debris: 0f, repair: 0f, id: "instant"), _pool, _grid);
            var states = new List<BuildingState>();
            building.OnStateChanged += (sender, from, to) => states.Add(to);
            Assert.That(building.StartRepair(), Is.True);
            Assert.That(states, Is.EqualTo(new[] { BuildingState.Repairing, BuildingState.Unprepared }));
        }

        [Test]
        public void Preparation_RequiresRepairedBuildingAndFailureDoesNotStart()
        {
            Assert.Throws<InvalidOperationException>(() => _building.ApplyPreparation(PreparationResult.Ok(1f)));
            _building.StartRepair();
            Assert.Throws<InvalidOperationException>(() => _building.ApplyPreparation(PreparationResult.Ok(1f)));
            _building.Tick(2f);
            _building.ApplyPreparation(PreparationResult.Fail("prep.test.failed"));
            _building.SetWorkerPresent(true);
            _grid.Tick(1f, false);
            _building.Tick(10f);
            Assert.That(_building.State, Is.EqualTo(BuildingState.Unprepared));
            Assert.That(_building.Yield, Is.Zero);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(10f));
        }

        [Test]
        public void Running_ProducesWithWorkerPowerAndYieldThenRequestsPreparationAgain()
        {
            Prepare(1.5f);
            _building.SetWorkerPresent(true);
            _grid.Tick(1f, false);
            _building.Tick(1f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(13f));
            Assert.Throws<InvalidOperationException>(() => _building.ApplyPreparation(PreparationResult.Ok(2f)));
            _building.Tick(100f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(22f));
            Assert.That(_building.State, Is.EqualTo(BuildingState.Depleted));
            _grid.Tick(1f, false);
            Assert.That(_grid.IsPowered(_building.Id), Is.False);
            _building.Tick(100f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(22f));
            _building.ApplyPreparation(PreparationResult.Ok(0.5f));
            _grid.Tick(1f, false);
            _building.Tick(1f);
            Assert.That(_building.State, Is.EqualTo(BuildingState.Running));
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(23f));
            Assert.That(_pool.Get(ResourceId.Metal), Is.EqualTo(8f));
        }

        [Test]
        public void MissingWorker_PausesCycleAndDoesNotDrawEnergy()
        {
            Prepare();
            _grid.Tick(10f, false);
            _building.Tick(10f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(10f));
            Assert.That(_pool.Get(ResourceId.Electricity), Is.EqualTo(10f));
            Assert.That(_building.State, Is.EqualTo(BuildingState.Running));
            _building.SetWorkerPresent(true);
            _grid.Tick(1f, false);
            _building.Tick(1f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(12f));
            _building.SetWorkerPresent(false);
            _grid.Tick(10f, false);
            _building.Tick(10f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(12f));
            Assert.That(_pool.Get(ResourceId.Electricity), Is.EqualTo(9f));
            _building.SetWorkerPresent(true);
            _grid.Tick(3f, false);
            _building.Tick(3f);
            Assert.That(_building.State, Is.EqualTo(BuildingState.Depleted));
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(18f));
        }

        [Test]
        public void Blackout_PausesCycleAndRestoredPowerResumesIt()
        {
            Prepare();
            _building.SetWorkerPresent(true);
            _pool.TryConsume(ResourceId.Electricity, 10f);
            _grid.Tick(1f, false);
            _building.Tick(100f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(10f));
            Assert.That(_building.State, Is.EqualTo(BuildingState.Running));
            _pool.Add(ResourceId.Electricity, 10f);
            _grid.Tick(4f, false);
            _building.Tick(4f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(18f));
            Assert.That(_building.State, Is.EqualTo(BuildingState.Depleted));
        }

        [Test]
        public void FullPool_PreservesProductionAcrossDepletionAndNewPreparation()
        {
            _pool.Add(ResourceId.Food, 89f);
            Prepare();
            _building.SetWorkerPresent(true);
            _grid.Tick(4f, false);
            _building.Tick(4f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(100f));
            Assert.That(_building.State, Is.EqualTo(BuildingState.Depleted));
            _building.ApplyPreparation(PreparationResult.Fail("prep.test.failed"));
            _pool.TryConsume(ResourceId.Food, 10f);
            _building.Tick(1f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(97f));
            _building.Tick(1f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(97f));
        }

        [Test]
        public void Events_ReportEveryTransitionAfterUpdatingState()
        {
            var transitions = new List<string>();
            _building.OnStateChanged += (sender, from, to) =>
            {
                Assert.That(sender, Is.EqualTo(_building));
                Assert.That(sender.State, Is.EqualTo(to));
                transitions.Add(from + ">" + to);
            };
            Prepare();
            _building.SetWorkerPresent(true);
            _grid.Tick(4f, false);
            _building.Tick(4f);
            _building.ApplyPreparation(PreparationResult.Fail("prep.test.failed"));
            Assert.That(transitions, Is.EqualTo(new[] { "Damaged>Repairing", "Repairing>Unprepared",
                "Unprepared>Running", "Running>Depleted", "Depleted>Unprepared" }));
        }

        [Test]
        public void ProductionEvent_ReentrantTickDoesNotDuplicateProduction()
        {
            Prepare();
            _building.SetWorkerPresent(true);
            _grid.Tick(1f, false);
            _pool.OnChanged += (id, amount) =>
            {
                if (id == ResourceId.Food)
                    _building.Tick(1f);
            };
            _building.Tick(1f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(12f));
        }

        [Test]
        public void ZeroYield_PreparationCanStartWithoutProducing()
        {
            Prepare(0f);
            _building.SetWorkerPresent(true);
            _grid.Tick(4f, false);
            _building.Tick(4f);
            Assert.That(_pool.Get(ResourceId.Food), Is.EqualTo(10f));
            Assert.That(_building.State, Is.EqualTo(BuildingState.Depleted));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Tick_RejectsInvalidTime(float invalid)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _building.Tick(invalid));
        }

        [Test]
        public void ZeroTick_DoesNotAdvanceRepair()
        {
            _building.StartRepair();
            _building.Tick(0f);
            Assert.That(_building.State, Is.EqualTo(BuildingState.Repairing));
        }

        [Test]
        public void Constructor_RejectsNullDependenciesAndDuplicateConsumer()
        {
            Assert.Throws<ArgumentNullException>(() => new Building(null, _pool, _grid));
            Assert.Throws<ArgumentNullException>(() => new Building(BuildingConfigTests.Config(), null, _grid));
            Assert.Throws<ArgumentNullException>(() => new Building(BuildingConfigTests.Config(), _pool, null));
            Assert.Throws<ArgumentException>(() => new Building(BuildingConfigTests.Config(), _pool, _grid));
        }
    }
}
