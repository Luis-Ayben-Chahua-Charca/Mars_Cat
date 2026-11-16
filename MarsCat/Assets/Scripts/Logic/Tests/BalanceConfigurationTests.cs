// Asistencia de IA: escrito con Codex (OpenAI); modificado con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Clock;
using MarsCat.Logic.Data;
using MarsCat.Logic.Resources;
using MarsCat.Logic.Work;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class BalanceConfigurationTests
    {
        private const string Header = "clave;valor;unidad;fuente\n";

        [Test]
        public void ClockReadsStartProgressAndRequiresAllKeys()
        {
            var data = BalanceLoader.Parse(Header +
                "clock.day_seconds;12;s;diseno\nclock.night_seconds;4;s;diseno\n" +
                "clock.night_time_scale;2;factor;diseno\nclock.start_progress;0.25;fraccion;diseno");
            var config = DayNightConfig.FromBalance(data);
            Assert.That(config.DaySeconds, Is.EqualTo(12f));
            Assert.That(config.NightSeconds, Is.EqualTo(4f));
            Assert.That(config.NightTimeScale, Is.EqualTo(2f));
            Assert.That(config.StartProgress, Is.EqualTo(0.25f));
            Assert.That(Assert.Throws<KeyNotFoundException>(() =>
                DayNightConfig.FromBalance(BalanceLoader.Parse(Header))).Message,
                Does.Contain("clock.day_seconds"));
        }

        [Test]
        public void CatsKeepRealSecondsAndReadRationsAndSpread()
        {
            var data = BalanceLoader.Parse(CatCsv());
            var config = CatConfig.FromBalance(data);
            Assert.That(config.FoodIntervalSeconds, Is.EqualTo(2f));
            Assert.That(config.FoodAlertSeconds, Is.EqualTo(4f));
            Assert.That(config.FoodDeathSeconds, Is.EqualTo(5f));
            Assert.That(config.FoodRation, Is.EqualTo(3f));
            Assert.That(config.WaterIntervalSeconds, Is.EqualTo(1f));
            Assert.That(config.WaterAlertSeconds, Is.EqualTo(2f));
            Assert.That(config.WaterDeathSeconds, Is.EqualTo(3f));
            Assert.That(config.WaterRation, Is.EqualTo(4f));
            Assert.That(config.InitialNeedSpread, Is.EqualTo(0.2f));
        }

        [Test]
        public void CatsRejectMissingRationAndInvalidNeedOrder()
        {
            string csv = CatCsv().Replace("cat.food.ration;3;u;diseno\n", "");
            Assert.That(Assert.Throws<KeyNotFoundException>(() =>
                CatConfig.FromBalance(BalanceLoader.Parse(csv))).Message,
                Does.Contain("cat.food.ration"));
            Assert.Throws<ArgumentException>(() => CatConfig.FromBalance(
                BalanceLoader.Parse(CatCsv().Replace("interval_seconds;2", "interval_seconds;4"))));
        }

        [Test]
        public void ResourcesReadEveryFiniteResourceAndKeepInfiniteResources()
        {
            var config = ResourceConfig.FromBalance(BalanceLoader.Parse(ResourceCsv()));
            foreach (ResourceId id in Enum.GetValues(typeof(ResourceId)))
            {
                bool infinite = id == ResourceId.Bacteria || id == ResourceId.Regolith;
                Assert.That(config.GetInitialAmount(id), Is.EqualTo(infinite ? float.PositiveInfinity : (float)(int)id));
                Assert.That(config.GetCapacity(id), Is.EqualTo(infinite ? float.PositiveInfinity : 20f));
            }
        }

        [Test]
        public void ResourcesRequireExplicitCapacityAndValidateAmounts()
        {
            string csv = ResourceCsv().Replace("resource.water.capacity;20;u;diseno\n", "");
            Assert.That(Assert.Throws<KeyNotFoundException>(() =>
                ResourceConfig.FromBalance(BalanceLoader.Parse(csv))).Message,
                Does.Contain("resource.water.capacity"));
            Assert.Throws<ArgumentException>(() => ResourceConfig.FromBalance(
                BalanceLoader.Parse(ResourceCsv().Replace("resource.water.initial;2", "resource.water.initial;21"))));
        }

        [Test]
        public void WorkReadsSharedBuildingPrioritiesAndIgnoresOtherKeys()
        {
            var config = WorkConfig.FromBalance(BalanceLoader.Parse(Header +
                "building.farm.energy_priority;0;grupo;diseno\n" +
                "building.water.energy_priority;1;grupo;diseno\n" +
                "building.moxie.energy_priority;2;grupo;diseno\n" +
                "building.farm.run_seconds;10;s;diseno\n"));
            var assigner = new JobAssigner(config);
            var pool = new ResourcePool(new ResourceConfig(ResourceConfigTests.FiniteValues(0f),
                ResourceConfigTests.FiniteValues(100f)));
            var colony = new CatColony(CatTests.Config(), pool, 0);
            colony.Spawn(3);
            var released = new List<string>();
            assigner.OnReleased += (job, cat) => released.Add(job.BuildingId);
            foreach (string id in new[] { "moxie", "water", "farm" })
                assigner.RegisterJob(new Job(id, JobKind.Post, id, false));
            assigner.Tick(0f, colony, false);
            for (int i = 0; i < 3; i++)
            {
                assigner.RegisterJob(new Job("rescue" + i, JobKind.OneOff, "moxie", false, TaskPriority.Vital));
                assigner.Tick(0f, colony, false);
            }
            Assert.That(released, Is.EqualTo(new[] { "farm", "water", "moxie" }));
            Assert.Throws<KeyNotFoundException>(() =>
                assigner.RegisterJob(new Job("missing", JobKind.Post, "missing", false)));
        }

        [TestCase("-1")]
        [TestCase("1.5")]
        [TestCase("3")]
        public void WorkRejectsUnknownOrFractionalPriorities(string priority)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => WorkConfig.FromBalance(
                BalanceLoader.Parse(Header + "building.farm.energy_priority;" + priority + ";grupo;diseno")));
        }

        [TestCase("building..energy_priority")]
        [TestCase("building.energy_priority")]
        public void WorkRejectsEmptyBuildingIds(string key)
        {
            Assert.Throws<ArgumentException>(() => WorkConfig.FromBalance(
                BalanceLoader.Parse(Header + key + ";0;grupo;diseno")));
        }

        [Test]
        public void FactoriesRejectNullAndWorkRequiresPriorities()
        {
            Assert.Throws<ArgumentNullException>(() => DayNightConfig.FromBalance(null));
            Assert.Throws<ArgumentNullException>(() => CatConfig.FromBalance(null));
            Assert.Throws<ArgumentNullException>(() => ResourceConfig.FromBalance(null));
            Assert.Throws<ArgumentNullException>(() => WorkConfig.FromBalance(null));
            Assert.That(Assert.Throws<KeyNotFoundException>(() =>
                WorkConfig.FromBalance(BalanceLoader.Parse(Header))).Message,
                Does.Contain("building.<id>.energy_priority"));
        }

        private static string CatCsv()
        {
            // Valores propios de prueba, sin atribución a NASA ni al balance del juego.
            return Header + "cat.food.interval_seconds;2;s;diseno\n" +
                "cat.food.alert_seconds;4;s;diseno\ncat.food.death_seconds;5;s;diseno\n" +
                "cat.food.ration;3;u;diseno\ncat.water.interval_seconds;1;s;diseno\n" +
                "cat.water.alert_seconds;2;s;diseno\ncat.water.death_seconds;3;s;diseno\n" +
                "cat.water.ration;4;u;diseno\ncat.initial_need_spread;0.2;fraccion;diseno\n";
        }

        private static string ResourceCsv()
        {
            string[] names = { "electricity", "food", "water", "metal", "debris",
                "compost", "cat_waste", "rat_waste", "crop_residue",
                null, null, // Bacteria y Regolith: infinitos, sin claves
                "forage", "fiber" };
            var csv = new StringBuilder(Header);
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == null)
                    continue;
                csv.Append("resource.").Append(names[i]).Append(".initial;")
                    .Append(i.ToString(CultureInfo.InvariantCulture)).Append(";u;diseno\n");
                csv.Append("resource.").Append(names[i]).Append(".capacity;20;u;diseno\n");
            }
            return csv.ToString();
        }
    }
}
