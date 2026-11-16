// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Recycling;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class CompostSessionTests
    {
        private static ResourcePool Pool(float electricity = 10f, float water = 10f)
        {
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.Electricity] = electricity;
            amounts[ResourceId.Water] = water;
            return new ResourcePool(new ResourceConfig(amounts, ResourceConfigTests.FiniteValues(100f)));
        }

        // Valores de prueba (no son datos de NASA): ambiente 10 °C, calor al 100 % apunta a 80 °C.
        private static CompostConfig Config(float cycle = 60f, float startTemp = 60f, float ambient = 10f,
            float startHumidity = 55f, float drying = 0.1f)
        {
            return new CompostConfig(55f, 65f, 70f, 50f, 60f, 65f, 40f,
                0.5f, 0.3f, 0.005f, 0.01f, cycle, 1f, 0.6f,
                startTemp, ambient, 80f, startHumidity, 2f, drying);
        }

        private static void Run(CompostSession session, float seconds, float step = 1f)
        {
            for (float t = 0f; t < seconds && !session.IsFinished; t += step)
                session.Tick(step);
        }

        [Test]
        public void WithoutHeatTheTemperatureFallsAndNeverGoesBelowTheAmbient()
        {
            var session = new CompostSession(Config(cycle: 10000f), Pool());
            session.Start(1f);
            session.Tick(10f);
            Assert.That(session.Temperature, Is.LessThan(60f));
            float after10 = session.Temperature;
            session.Tick(10f);
            Assert.That(session.Temperature, Is.LessThan(after10));
            Run(session, 600f);
            Assert.That(session.Temperature, Is.EqualTo(10f).Within(0.01f));
        }

        [Test]
        public void ThermalInertiaDelaysTheRiseTowardTheKnob()
        {
            var session = new CompostSession(Config(cycle: 10000f, startTemp: 20f), Pool());
            session.Start(1f);
            session.SetHeat(1f);
            session.Tick(1f);
            // Apunta a 80 °C pero en 1 s solo cubre una fracción de la distancia (menos el frío constante).
            Assert.That(session.Temperature, Is.EqualTo(35.05f).Within(0.3f));
            float after1 = session.Temperature;
            session.Tick(1f);
            Assert.That(session.Temperature, Is.GreaterThan(after1));
            Assert.That(session.Temperature, Is.LessThan(80f));
        }

        [Test]
        public void HeatSpendsElectricityProportionalToTheLevel()
        {
            ResourcePool pool = Pool(electricity: 10f);
            var session = new CompostSession(Config(cycle: 10000f), pool);
            session.Start(1f);
            session.SetHeat(0.5f);
            session.Tick(10f);
            // 0,005 kWh/s al 100 %: al 50 % durante 10 s son 0,025 kWh.
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(10f - 0.025f).Within(1e-3f));
        }

        [Test]
        public void WithoutElectricityTheHeatHasNoEffect()
        {
            var warm = new CompostSession(Config(cycle: 10000f, startTemp: 20f), Pool(electricity: 10f));
            var cold = new CompostSession(Config(cycle: 10000f, startTemp: 20f), Pool(electricity: 0f));
            warm.Start(1f);
            cold.Start(1f);
            warm.SetHeat(1f);
            cold.SetHeat(1f);
            warm.Tick(5f);
            cold.Tick(5f);
            Assert.That(warm.Temperature, Is.GreaterThan(20f));
            Assert.That(cold.Temperature, Is.LessThan(20f));
        }

        [Test]
        public void SprinklerSpendsWaterAndRaisesTheHumidity()
        {
            ResourcePool pool = Pool(water: 10f);
            var session = new CompostSession(Config(cycle: 10000f, drying: 0f), pool);
            session.Start(1f);
            session.SetSprinkler(true);
            session.Tick(4f);
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(10f - 0.04f).Within(1e-3f));
            Assert.That(session.Humidity, Is.EqualTo(63f).Within(0.01f));
            Assert.That(session.IsDrowning, Is.False);
            session.Tick(2f);
            Assert.That(session.IsDrowning, Is.True);
            session.SetSprinkler(false);
            float water = pool.Get(ResourceId.Water);
            session.Tick(5f);
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(water));
        }

        [Test]
        public void WithoutWaterTheSprinklerDoesNothing()
        {
            var session = new CompostSession(Config(cycle: 10000f, drying: 0f), Pool(water: 0f));
            session.Start(1f);
            session.SetSprinkler(true);
            session.Tick(5f);
            Assert.That(session.Humidity, Is.EqualTo(55f).Within(1e-4f));
        }

        [Test]
        public void HumidityDriesAndTheDryFlagTurnsOn()
        {
            var session = new CompostSession(Config(cycle: 10000f, startHumidity: 41f, drying: 1f), Pool());
            session.Start(1f);
            Assert.That(session.IsTooDry, Is.False);
            session.Tick(2f);
            Assert.That(session.IsTooDry, Is.True);
        }

        [Test]
        public void AboveTheOverheatTemperatureTheBatchIsLostAndFinishesOnce()
        {
            var session = new CompostSession(Config(cycle: 10000f), Pool(electricity: 90f));
            int finished = 0;
            PreparationResult result = default;
            session.OnFinished += r => { finished++; result = r; };
            session.Start(1f);
            session.SetHeat(1f);
            Run(session, 100f);
            Assert.That(session.IsFinished, Is.True);
            Assert.That(finished, Is.EqualTo(1));
            Assert.That(result.Starts, Is.False);
            Assert.That(result.ReasonKey, Is.EqualTo("prep.compost.overheated"));
            Assert.That(session.Result.ReasonKey, Is.EqualTo("prep.compost.overheated"));
            session.Tick(10f);
            Assert.That(finished, Is.EqualTo(1));
        }

        [Test]
        public void ABigTickDoesNotSkipTheOverheat()
        {
            var session = new CompostSession(Config(cycle: 10000f), Pool(electricity: 90f));
            session.Start(1f);
            session.SetHeat(1f);
            session.Tick(100f);
            Assert.That(session.IsFinished, Is.True);
            Assert.That(session.Result.Starts, Is.False);
        }

        // Ambiente en la zona verde y sin secado: las dos agujas se quedan en verde todo el ciclo.
        private static CompostSession GreenSession(ResourcePool pool)
        {
            return new CompostSession(Config(cycle: 60f, startTemp: 60f, ambient: 60f, startHumidity: 55f, drying: 0f), pool);
        }

        [Test]
        public void AFullCycleInGreenYieldsOne()
        {
            var session = GreenSession(Pool());
            PreparationResult result = default;
            session.OnFinished += r => result = r;
            session.Start(1f);
            Run(session, 60f);
            Assert.That(session.IsFinished, Is.True);
            Assert.That(session.TimeInGreen, Is.EqualTo(60f).Within(1e-3f));
            Assert.That(result.Starts, Is.True);
            Assert.That(result.Yield, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void TheMixScoreMultipliesTheFinalYield()
        {
            var session = GreenSession(Pool());
            session.Start(0.5f);
            Run(session, 60f);
            Assert.That(session.Result.Yield, Is.EqualTo(0.5f).Within(1e-4f));
        }

        [Test]
        public void OnlyTheTimeWithBothNeedlesInGreenCounts()
        {
            // La humedad sale del verde (50) a los 5 s y no vuelve.
            var session = new CompostSession(
                Config(cycle: 60f, startTemp: 60f, ambient: 60f, startHumidity: 55f, drying: 1f), Pool());
            session.Start(1f);
            Run(session, 60f);
            Assert.That(session.TimeInGreen, Is.EqualTo(5f).Within(0.2f));
            Assert.That(session.Result.Yield, Is.EqualTo(5f / 60f).Within(0.005f));
        }

        [Test]
        public void TemperatureOutOfGreenDoesNotCount()
        {
            var session = new CompostSession(
                Config(cycle: 60f, startTemp: 40f, ambient: 40f, startHumidity: 55f, drying: 0f), Pool());
            session.Start(1f);
            Run(session, 60f);
            Assert.That(session.TimeInGreen, Is.Zero);
            Assert.That(session.Result.Starts, Is.True);
            Assert.That(session.Result.Yield, Is.Zero);
        }

        [Test]
        public void ABigTickFinishesTheCycleExactly()
        {
            var session = GreenSession(Pool());
            int finished = 0;
            session.OnFinished += r => finished++;
            session.Start(1f);
            session.Tick(1000f);
            Assert.That(session.IsFinished, Is.True);
            Assert.That(finished, Is.EqualTo(1));
            Assert.That(session.TimeInGreen, Is.EqualTo(60f).Within(1e-3f));
        }

        [Test]
        public void StartResetsTheCycleAndCanRunAgainAfterFinishing()
        {
            var session = GreenSession(Pool());
            session.Start(1f);
            session.Tick(100f);
            Assert.That(session.IsFinished, Is.True);
            session.Start(0.5f);
            Assert.That(session.IsFinished, Is.False);
            Assert.That(session.TimeInGreen, Is.Zero);
            Assert.That(session.Temperature, Is.EqualTo(60f));
            session.Tick(100f);
            Assert.That(session.Result.Yield, Is.EqualTo(0.5f).Within(1e-4f));
        }

        [Test]
        public void StartWhileRunningThrows()
        {
            var session = GreenSession(Pool());
            session.Start(1f);
            Assert.Throws<InvalidOperationException>(() => session.Start(1f));
        }

        [Test]
        public void InvalidInputThrows()
        {
            var session = GreenSession(Pool());
            Assert.Throws<InvalidOperationException>(() => session.Tick(1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Start(1.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Start(-0.1f));
            session.Start(1f);
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.SetHeat(1.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.SetHeat(-0.1f));
            Assert.Throws<ArgumentNullException>(() => new CompostSession(null, Pool()));
            Assert.Throws<ArgumentNullException>(() => new CompostSession(Config(), null));
        }

        [Test]
        public void HeatKnobAndSprinklerStatePersistUntilChanged()
        {
            var session = GreenSession(Pool());
            session.Start(1f);
            session.SetHeat(0.4f);
            session.SetSprinkler(true);
            session.Tick(1f);
            Assert.That(session.HeatLevel, Is.EqualTo(0.4f));
            Assert.That(session.IsSprinklerOn, Is.True);
            Assert.That(session.IsRunning, Is.True);
        }
    }
}
