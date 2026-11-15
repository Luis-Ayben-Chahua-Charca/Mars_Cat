using System;
using System.Collections.Generic;
using System.Linq;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class CatColonyTests
    {
        private static ResourcePool Pool(float food, float water)
        {
            var initial = ResourceConfigTests.FiniteValues(0f);
            initial[ResourceId.Food] = food;
            initial[ResourceId.Water] = water;
            return new ResourcePool(new ResourceConfig(initial, ResourceConfigTests.FiniteValues(10000f)));
        }

        [Test]
        public void Spawn_SeedReproducesIndependentNeedsAndUniqueIds()
        {
            var first = new CatColony(CatTests.Config(0.3f), Pool(0f, 0f), 17);
            var second = new CatColony(CatTests.Config(0.3f), Pool(0f, 0f), 17);
            first.Spawn(15);
            second.Spawn(7);
            second.Spawn(8);
            for (int i = 0; i < 15; i++)
            {
                Assert.That(first.Cats[i].Id, Is.EqualTo(i));
                Assert.That(first.Cats[i].Hunger, Is.InRange(0f, 0.3f));
                Assert.That(first.Cats[i].Thirst, Is.InRange(0f, 0.3f));
                Assert.That(first.Cats[i].Hunger, Is.EqualTo(second.Cats[i].Hunger));
                Assert.That(first.Cats[i].Thirst, Is.EqualTo(second.Cats[i].Thirst));
            }
            Assert.That(first.Cats.Select(cat => cat.Hunger).Distinct().Count(), Is.GreaterThan(1));
            Assert.That(first.Cats.Any(cat => cat.Hunger != cat.Thirst), Is.True);
        }

        [Test]
        public void Tick_ConsumesAtIntervalsAndKeepsWorking()
        {
            var pool = Pool(10f, 30f);
            var colony = new CatColony(CatTests.Config(), pool, 0);
            colony.Spawn(1);
            colony.Cats[0].TryAssign(false);
            colony.Tick(119f, false);
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(30f));
            colony.Tick(1f, false);
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(27f));
            Assert.That(colony.Cats[0].Thirst, Is.Zero);
            colony.Tick(120f, false);
            Assert.That(pool.Get(ResourceId.Food), Is.EqualTo(8f));
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(24f));
            Assert.That(colony.Cats[0].Hunger, Is.Zero);
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.WorkingIndoor));
        }

        [TestCase(true, 480f, 600f, Need.Food)]
        [TestCase(false, 240f, 300f, Need.Water)]
        public void Tick_AlertsOnceReleasesWorkAndDiesAtDeadline(bool starveFood,
            float alertTime, float deathTime, Need expectedNeed)
        {
            var colony = new CatColony(CatTests.Config(), Pool(starveFood ? 0f : 100f,
                starveFood ? 100f : 0f), 0);
            colony.Spawn(1);
            Cat cat = colony.Cats[0];
            var alerts = new List<Need>();
            var transitions = new List<CatState>();
            int deaths = 0;
            colony.OnExtremeAlert += (changed, need) => alerts.Add(need);
            colony.OnDied += changed => deaths++;
            colony.OnStateChanged += (changed, from, to) => transitions.Add(to);
            cat.TryAssign(true);
            colony.Tick(alertTime, false);
            Assert.That(cat.State, Is.EqualTo(CatState.SeekingFood));
            Assert.That(alerts, Is.EqualTo(new[] { expectedNeed }));
            Assert.That(cat.TryAssign(false), Is.False);
            cat.Release();
            Assert.That(cat.State, Is.EqualTo(CatState.SeekingFood));
            colony.Tick(deathTime - alertTime, false);
            colony.Tick(1000f, false);
            Assert.That(cat.State, Is.EqualTo(CatState.Dead));
            Assert.That(cat.Health, Is.Zero);
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(alerts.Count, Is.EqualTo(1));
            Assert.That(transitions, Is.EqualTo(new[] {
                CatState.WorkingOutdoor, CatState.SeekingFood, CatState.Dead }));
            Assert.That(colony.FreeCats.Count(), Is.Zero);
        }

        [Test]
        public void Tick_LimitedFoodFeedsHungriestEvenIfListedLast()
        {
            var pool = Pool(0f, 100f);
            var colony = new CatColony(CatTests.Config(0.3f), pool, 17);
            colony.Spawn(15);
            colony.Tick(240f, false);
            Cat hungriest = colony.Cats.OrderByDescending(cat => cat.Hunger).First();
            float previousHunger = colony.Cats.Sum(cat => cat.Hunger);
            pool.Add(ResourceId.Food, 2f);
            colony.Tick(0f, false);
            Assert.That(hungriest.Hunger, Is.Zero);
            Assert.That(colony.Cats.Count(cat => cat.Hunger == 0f), Is.EqualTo(1));
            Assert.That(colony.Cats.Sum(cat => cat.Hunger), Is.LessThan(previousHunger));
            Assert.That(pool.Get(ResourceId.Food), Is.Zero);
        }

        [Test]
        public void Tick_LimitedWaterFeedsThirstiestAndNeverConsumesPartialRations()
        {
            var pool = Pool(100f, 0f);
            var colony = new CatColony(CatTests.Config(0.3f), pool, 17);
            colony.Spawn(15);
            colony.Tick(120f, false);
            Cat thirstiest = colony.Cats.OrderByDescending(cat => cat.Thirst).First();
            float previousThirst = thirstiest.Thirst;
            pool.Add(ResourceId.Water, 2f);
            colony.Tick(0f, false);
            Assert.That(thirstiest.Thirst, Is.EqualTo(previousThirst));
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(2f));
            pool.Add(ResourceId.Water, 1f);
            colony.Tick(0f, false);
            Assert.That(thirstiest.Thirst, Is.Zero);
            Assert.That(colony.Cats.Count(cat => cat.Thirst == 0f), Is.EqualTo(1));
            Assert.That(pool.Get(ResourceId.Water), Is.Zero);
        }

        [Test]
        public void Tick_FractionalStepsPreserveRepeatedMealsAndThresholds()
        {
            var config = new CatConfig(0.25f, 0.5f, 0.75f, 1f,
                0.125f, 0.25f, 0.375f, 1f, 0.3f);
            var firstPool = Pool(20f, 40f);
            var secondPool = Pool(20f, 40f);
            var first = new CatColony(config, firstPool, 17);
            var second = new CatColony(config, secondPool, 17);
            first.Spawn(3);
            second.Spawn(3);
            first.Tick(2f, false);
            for (int i = 0; i < 16; i++)
                second.Tick(0.125f, false);
            for (int i = 0; i < 3; i++)
            {
                Assert.That(first.Cats[i].Hunger, Is.EqualTo(second.Cats[i].Hunger).Within(0.00001f));
                Assert.That(first.Cats[i].Thirst, Is.EqualTo(second.Cats[i].Thirst).Within(0.00001f));
                Assert.That(first.Cats[i].State, Is.EqualTo(second.Cats[i].State));
            }
            Assert.That(firstPool.Get(ResourceId.Food), Is.EqualTo(secondPool.Get(ResourceId.Food)));
            Assert.That(firstPool.Get(ResourceId.Water), Is.EqualTo(secondPool.Get(ResourceId.Water)));
        }

        [Test]
        public void Tick_NightRestsWorkersAndNewCatsAndDawnFreesThem()
        {
            var colony = new CatColony(CatTests.Config(), Pool(100f, 100f), 0);
            colony.Spawn(1);
            colony.Cats[0].TryAssign(false);
            colony.Tick(60f, true);
            colony.Spawn(1);
            Assert.That(colony.Cats.All(cat => cat.State == CatState.Resting), Is.True);
            Assert.That(colony.Cats[0].Hunger, Is.EqualTo(0.1f).Within(0.00001f));
            Assert.That(colony.Cats[0].Thirst, Is.EqualTo(0.2f).Within(0.00001f));
            Assert.That(colony.Cats[0].TryAssign(false), Is.False);
            colony.Tick(0f, false);
            Assert.That(colony.FreeCats.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Tick_RecoveryRequiresBothNeedsAndAlertsCanRepeatAfterFeeding()
        {
            var pool = Pool(0f, 0f);
            var config = new CatConfig(2f, 4f, 10f, 1f, 2f, 4f, 10f, 1f, 0f);
            var colony = new CatColony(config, pool, 0);
            colony.Spawn(1);
            int alerts = 0;
            colony.OnExtremeAlert += (cat, need) => alerts++;
            colony.Tick(4f, true);
            Assert.That(alerts, Is.EqualTo(2));
            pool.Add(ResourceId.Food, 1f);
            colony.Tick(0f, true);
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.SeekingFood));
            pool.Add(ResourceId.Water, 1f);
            colony.Tick(0f, true);
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.Resting));
            colony.Tick(4f, true);
            Assert.That(alerts, Is.EqualTo(4));
        }

        [Test]
        public void Tick_LargeTickMatchesSmallTicksAndDoesNotResurrectDeadCats()
        {
            var firstPool = Pool(8f, 21f);
            var secondPool = Pool(8f, 21f);
            var first = new CatColony(CatTests.Config(), firstPool, 17);
            var second = new CatColony(CatTests.Config(), secondPool, 17);
            first.Spawn(1);
            second.Spawn(1);
            first.Tick(1200f, false);
            for (int i = 0; i < 1200; i++)
                second.Tick(1f, false);
            Assert.That(first.Cats[0].State, Is.EqualTo(second.Cats[0].State));
            Assert.That(first.Cats[0].Hunger, Is.EqualTo(second.Cats[0].Hunger));
            Assert.That(first.Cats[0].Thirst, Is.EqualTo(second.Cats[0].Thirst));
            Assert.That(firstPool.Get(ResourceId.Food), Is.EqualTo(secondPool.Get(ResourceId.Food)));
            Assert.That(firstPool.Get(ResourceId.Water), Is.EqualTo(secondPool.Get(ResourceId.Water)));
            first.Tick(2000f, false);
            Assert.That(first.Cats[0].State, Is.EqualTo(CatState.Dead));
            firstPool.Add(ResourceId.Food, 100f);
            firstPool.Add(ResourceId.Water, 100f);
            first.Tick(0f, false);
            Assert.That(first.Cats[0].State, Is.EqualTo(CatState.Dead));
            Assert.That(firstPool.Get(ResourceId.Food), Is.EqualTo(100f));
        }

        [Test]
        public void ConstructorSpawnAndTick_RejectInvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new CatColony(null, Pool(0f, 0f), 0));
            Assert.Throws<ArgumentNullException>(() => new CatColony(CatTests.Config(), null, 0));
            var colony = new CatColony(CatTests.Config(), Pool(0f, 0f), 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => colony.Spawn(-1));
            foreach (float invalid in new[] { -1f, float.NaN, float.PositiveInfinity })
                Assert.Throws<ArgumentOutOfRangeException>(() => colony.Tick(invalid, false));
            colony.Tick(10f, false);
            Assert.That(colony.Cats.Count, Is.Zero);
        }
    }
}
