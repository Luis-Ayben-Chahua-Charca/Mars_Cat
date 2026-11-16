using System;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Data;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class CatWasteTests
    {
        private static ResourcePool Pool(float food)
        {
            var initial = ResourceConfigTests.FiniteValues(0f);
            initial[ResourceId.Food] = food;
            initial[ResourceId.Water] = 100f;
            return new ResourcePool(new ResourceConfig(initial, ResourceConfigTests.FiniteValues(10000f)));
        }

        private static CatConfig Config(float waste)
        {
            return new CatConfig(240f, 480f, 600f, 2f, 120f, 240f, 300f, 3f, 0f, waste);
        }

        [Test]
        public void Meal_AddsWastePerMealToPool()
        {
            var pool = Pool(10f);
            var colony = new CatColony(Config(0.018f), pool, 0);
            colony.Spawn(1);
            colony.Tick(240f, false);
            Assert.That(pool.Get(ResourceId.Food), Is.EqualTo(8f));
            Assert.That(pool.Get(ResourceId.CatWaste), Is.EqualTo(0.018f).Within(1e-6f));
        }

        [Test]
        public void MealWithoutRations_AddsNoWaste()
        {
            var pool = Pool(0f);
            var colony = new CatColony(Config(0.018f), pool, 0);
            colony.Spawn(1);
            colony.Tick(240f, false);
            Assert.That(pool.Get(ResourceId.CatWaste), Is.Zero);
        }

        [Test]
        public void Drinking_AddsNoWaste()
        {
            var pool = Pool(0f);
            var colony = new CatColony(Config(0.018f), pool, 0);
            colony.Spawn(1);
            colony.Tick(120f, false);
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(97f));
            Assert.That(pool.Get(ResourceId.CatWaste), Is.Zero);
        }

        [Test]
        public void DefaultConfig_HasNoWaste()
        {
            Assert.That(CatTests.Config().WastePerMeal, Is.Zero);
        }

        [Test]
        public void Config_RejectsNegativeWaste()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(-0.1f));
        }

        [Test]
        public void FromBalance_ReadsWastePerMeal()
        {
            string csv = "clave;valor;unidad;fuente\n" +
                "cat.food.interval_seconds;2;s;diseno\ncat.food.alert_seconds;4;s;diseno\n" +
                "cat.food.death_seconds;5;s;diseno\ncat.food.ration;3;u;diseno\n" +
                "cat.water.interval_seconds;1;s;diseno\ncat.water.alert_seconds;2;s;diseno\n" +
                "cat.water.death_seconds;3;s;diseno\ncat.water.ration;4;u;diseno\n" +
                "cat.initial_need_spread;0.2;fraccion;diseno\ncat.waste_per_meal;0.5;kg;diseno\n";
            Assert.That(CatConfig.FromBalance(BalanceLoader.Parse(csv)).WastePerMeal, Is.EqualTo(0.5f));
        }
    }
}
