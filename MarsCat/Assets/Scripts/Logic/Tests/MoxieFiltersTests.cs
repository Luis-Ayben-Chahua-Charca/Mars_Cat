// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Air;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Energy;
using MarsCat.Logic.Resources;
using MarsCat.Logic.Work;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class MoxieFiltersTests
    {
        private static MoxieFilters Filters()
        {
            return new MoxieFilters(new MoxieFilterConfig(5, 144f, 10f));
        }

        [Test]
        public void Tick_StartsCleanAndDirtiesOneFilterEvery144GameSeconds()
        {
            var filters = Filters();
            int dirty = 0;
            filters.OnFilterDirty += () => dirty++;
            Assert.That(filters.CleanFilters, Is.EqualTo(5));
            Assert.That(filters.Yield, Is.EqualTo(1f));
            filters.Tick(143f);
            Assert.That(dirty, Is.Zero);
            filters.Tick(1f);
            Assert.That(filters.CleanFilters, Is.EqualTo(4));
            Assert.That(filters.Yield, Is.EqualTo(0.8f));
            filters.Tick(576f);
            Assert.That(dirty, Is.EqualTo(5));
            Assert.That(filters.CleanFilters, Is.Zero);
            Assert.That(filters.Yield, Is.Zero);
            filters.Tick(float.MaxValue);
            Assert.That(dirty, Is.EqualTo(5));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void Cleaning_Restores20PercentPerFilterAndFinishesOnce(int count)
        {
            var filters = Filters();
            filters.Tick(720f);
            int finished = 0;
            int cleaned = -1;
            filters.OnCleaningFinished += value => { finished++; cleaned = value; };
            filters.RequestCleaning(count);
            filters.Tick(10f);
            Assert.That(filters.CleanFilters, Is.Zero);
            filters.SetWorkerPresent(true);
            filters.Tick(count * 10f - 1f);
            Assert.That(finished, Is.Zero);
            filters.Tick(1f);
            Assert.That(filters.CleanFilters, Is.EqualTo(count));
            Assert.That(filters.Yield, Is.EqualTo(count / 5f));
            Assert.That(cleaned, Is.EqualTo(count));
            Assert.That(finished, Is.EqualTo(1));
            filters.Tick(1f);
            Assert.That(finished, Is.EqualTo(1));
        }

        [Test]
        public void Cleaning_PausesWithoutWorkerAndPreservesPartialProgress()
        {
            var filters = Filters();
            filters.Tick(144f);
            filters.RequestCleaning(1);
            filters.SetWorkerPresent(true);
            filters.Tick(6f);
            filters.SetWorkerPresent(false);
            filters.Tick(144f);
            Assert.That(filters.CleanFilters, Is.EqualTo(3));
            filters.SetWorkerPresent(true);
            filters.Tick(3f);
            Assert.That(filters.CleanFilters, Is.EqualTo(3));
            filters.Tick(1f);
            Assert.That(filters.CleanFilters, Is.EqualTo(4));
        }

        [Test]
        public void Tick_FullDirtDoesNotBuildDebtAndIdleTimeDoesNotPrepayCleaning()
        {
            var filters = Filters();
            filters.Tick(1440f);
            filters.SetWorkerPresent(true);
            filters.Tick(144f);
            filters.RequestCleaning(1);
            filters.Tick(9f);
            Assert.That(filters.CleanFilters, Is.Zero);
            filters.Tick(1f);
            Assert.That(filters.CleanFilters, Is.EqualTo(1));
            filters.Tick(133f);
            Assert.That(filters.CleanFilters, Is.EqualTo(1));
            filters.Tick(1f);
            Assert.That(filters.CleanFilters, Is.Zero);
        }

        [Test]
        public void Cleaning_OnlyCompletedFiltersContributeToYield()
        {
            var filters = Filters();
            filters.Tick(720f);
            filters.RequestCleaning(2);
            filters.SetWorkerPresent(true);
            filters.Tick(9f);
            Assert.That(filters.Yield, Is.Zero);
            filters.Tick(1f);
            Assert.That(filters.Yield, Is.EqualTo(0.2f));
            filters.Tick(9f);
            Assert.That(filters.Yield, Is.EqualTo(0.2f));
            filters.Tick(1f);
            Assert.That(filters.Yield, Is.EqualTo(0.4f));
        }

        [Test]
        public void RequestCleaning_LimitsWorkToExistingDirtAndHandlesCleanMachine()
        {
            var filters = Filters();
            var results = new List<int>();
            filters.OnCleaningFinished += count => results.Add(count);
            filters.RequestCleaning(5);
            Assert.That(results, Is.EqualTo(new[] { 0 }));
            filters.Tick(144f);
            filters.RequestCleaning(5);
            filters.SetWorkerPresent(true);
            filters.Tick(10f);
            Assert.That(results, Is.EqualTo(new[] { 0, 1 }));
            Assert.That(filters.CleanFilters, Is.EqualTo(5));
        }

        [Test]
        public void Tick_LargeAndPartitionedStepsPreserveEventOrderAndResults()
        {
            var whole = new MoxieFilters(new MoxieFilterConfig(5, 4f, 2f));
            var split = new MoxieFilters(new MoxieFilterConfig(5, 4f, 2f));
            whole.Tick(20f);
            split.Tick(20f);
            var wholeEvents = new List<string>();
            var splitEvents = new List<string>();
            whole.OnFilterDirty += () => wholeEvents.Add("dirty");
            split.OnFilterDirty += () => splitEvents.Add("dirty");
            whole.OnCleaningFinished += count => wholeEvents.Add("clean:" + count);
            split.OnCleaningFinished += count => splitEvents.Add("clean:" + count);
            whole.RequestCleaning(5);
            split.RequestCleaning(5);
            whole.SetWorkerPresent(true);
            split.SetWorkerPresent(true);
            whole.Tick(30f);
            for (int i = 0; i < 120; i++)
                split.Tick(0.25f);
            Assert.That(whole.CleanFilters, Is.EqualTo(split.CleanFilters));
            Assert.That(wholeEvents, Is.EqualTo(splitEvents));
            Assert.That(wholeEvents, Does.Contain("clean:5"));
        }

        [Test]
        public void Tick_DirtEventCanRequestCleaningWithoutRetroactiveWork()
        {
            var filters = Filters();
            filters.SetWorkerPresent(true);
            filters.OnFilterDirty += () => filters.RequestCleaning(1);
            filters.Tick(149f);
            Assert.That(filters.CleanFilters, Is.EqualTo(4));
            filters.Tick(5f);
            Assert.That(filters.CleanFilters, Is.EqualTo(5));
        }

        [Test]
        public void Tick_CompletionCanReleaseWorkerAndQueueAnotherTask()
        {
            var filters = Filters();
            filters.Tick(720f);
            int finished = 0;
            filters.OnCleaningFinished += count =>
            {
                finished++;
                filters.SetWorkerPresent(false);
                filters.RequestCleaning(1);
                filters.Tick(10f);
            };
            filters.RequestCleaning(1);
            filters.SetWorkerPresent(true);
            filters.Tick(20f);
            Assert.That(finished, Is.EqualTo(1));
            Assert.That(filters.CleanFilters, Is.EqualTo(1));
            filters.SetWorkerPresent(true);
            filters.Tick(10f);
            Assert.That(finished, Is.EqualTo(2));
        }

        [Test]
        public void WorkIntegration_RequiresCatPausesAtNightAndReleasesWhenFinished()
        {
            var filters = Filters();
            filters.Tick(720f);
            var pool = new ResourcePool(new ResourceConfig(ResourceConfigTests.FiniteValues(0f),
                ResourceConfigTests.FiniteValues(100f)));
            var colony = new CatColony(CatTests.Config(), pool, 0);
            var assigner = new JobAssigner(new WorkConfig(new string[0], new string[0], new[] { "moxie" }));
            var job = new Job("moxie.clean", JobKind.OneOff, "moxie", false);
            assigner.OnAssigned += (assigned, cat) => filters.SetWorkerPresent(true);
            assigner.OnReleased += (released, cat) => filters.SetWorkerPresent(false);
            filters.OnCleaningFinished += count => assigner.UnregisterJob(job.Id);
            filters.RequestCleaning(1);
            assigner.RegisterJob(job);
            assigner.Tick(0f, colony, false);
            filters.Tick(10f);
            Assert.That(filters.CleanFilters, Is.Zero);
            colony.Spawn(1);
            assigner.Tick(0f, colony, false);
            filters.Tick(6f);
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.WorkingIndoor));
            assigner.Tick(0f, colony, true);
            filters.Tick(144f);
            Assert.That(filters.CleanFilters, Is.Zero);
            assigner.Tick(0f, colony, false);
            filters.Tick(4f);
            Assert.That(filters.CleanFilters, Is.EqualTo(1));
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.Free));
            assigner.Tick(0f, colony, false);
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.Free));
        }

        [Test]
        public void EnergyIntegration_DirtyFiltersKeepTheSameElectricityDemand()
        {
            // Tasas de prueba: el consumo nominal del MOXIE no se inventa para el balance.
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.Electricity] = 100f;
            var pool = new ResourcePool(new ResourceConfig(amounts, ResourceConfigTests.FiniteValues(100f)));
            var grid = new EnergyGrid(new EnergyConfig(720f, 0f), pool);
            var filters = Filters();
            grid.RegisterConsumer("moxie", 720f, EnergyPriority.Max);
            grid.Tick(10f, false);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(90f));
            filters.Tick(720f);
            Assert.That(filters.Yield, Is.Zero);
            grid.Tick(10f, false);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(80f));
            Assert.That(grid.IsPowered("moxie"), Is.True);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Tick_RejectsInvalidTimeWithoutChangingState(float dt)
        {
            var filters = Filters();
            Assert.Throws<ArgumentOutOfRangeException>(() => filters.Tick(dt));
            Assert.That(filters.CleanFilters, Is.EqualTo(5));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(3)]
        [TestCase(6)]
        public void RequestCleaning_RejectsUnsupportedCounts(int count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Filters().RequestCleaning(count));
        }

        [Test]
        public void RequestCleaning_RejectsOverlapWithoutLosingProgress()
        {
            var filters = Filters();
            filters.Tick(144f);
            filters.RequestCleaning(1);
            filters.SetWorkerPresent(true);
            filters.Tick(6f);
            Assert.Throws<InvalidOperationException>(() => filters.RequestCleaning(2));
            filters.Tick(4f);
            Assert.That(filters.CleanFilters, Is.EqualTo(5));
        }

        [Test]
        public void Constructor_RejectsNullAndZeroTickDoesNothing()
        {
            Assert.Throws<ArgumentNullException>(() => new MoxieFilters(null));
            var filters = Filters();
            filters.Tick(0f);
            Assert.That(filters.CleanFilters, Is.EqualTo(5));
        }
    }
}
