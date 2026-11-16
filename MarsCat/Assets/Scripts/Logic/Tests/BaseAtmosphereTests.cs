// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Air;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class BaseAtmosphereTests
    {
        // 720 s de juego por sol, densidad 1,2: 25 m3 con 21 % son 6,3 kg de O2.
        private static AtmosphereConfig Config(float initial = 0.21f, float newModule = 0.16f)
        {
            return new AtmosphereConfig(720f, initial, 0.195f, 0.16f, 0.1f,
                0.08f, 0.1f, 60f, 1.2f, newModule, 25f);
        }

        private static BaseAtmosphere Atmosphere(float initial = 0.21f, float volume = 25f)
        {
            return new BaseAtmosphere(Config(initial), volume);
        }

        [Test]
        public void Constructor_StartsWithInitialFractionAndGreenBand()
        {
            var air = Atmosphere();
            Assert.That(air.OxygenFraction, Is.EqualTo(0.21f).Within(1e-6f));
            Assert.That(air.OxygenKg, Is.EqualTo(6.3f).Within(1e-4f));
            Assert.That(air.VolumeM3, Is.EqualTo(25f));
            Assert.That(air.Band, Is.EqualTo(AirBand.Green));
        }

        [Test]
        public void Constructor_RejectsInvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new BaseAtmosphere(null, 25f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BaseAtmosphere(Config(), 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BaseAtmosphere(Config(), float.NaN));
        }

        [Test]
        public void Tick_WithoutProductionFractionDropsWithCats()
        {
            var air = Atmosphere();
            air.Tick(720f, 0f, 0, 10);
            // 10 gatos x 0,1 kg por sol = 1 kg de 6,3 kg.
            Assert.That(air.OxygenKg, Is.EqualTo(5.3f).Within(1e-4f));
            Assert.That(air.OxygenFraction, Is.EqualTo(5.3f / 30f).Within(1e-5f));
        }

        [Test]
        public void Tick_RestingCatsConsumeLessThanWorkingCats()
        {
            var resting = Atmosphere();
            var working = Atmosphere();
            resting.Tick(720f, 0f, 10, 0);
            working.Tick(720f, 0f, 0, 10);
            Assert.That(resting.OxygenKg, Is.EqualTo(6.3f - 0.8f).Within(1e-4f));
            Assert.That(resting.OxygenFraction, Is.GreaterThan(working.OxygenFraction));
        }

        [Test]
        public void Tick_ScalesWithGameDt()
        {
            var air = Atmosphere();
            air.Tick(360f, 0f, 0, 10);
            Assert.That(air.OxygenKg, Is.EqualTo(5.8f).Within(1e-4f));
        }

        [Test]
        public void Tick_BiggerVolumeDropsSlower()
        {
            var small = Atmosphere(volume: 25f);
            var big = Atmosphere(volume: 250f);
            small.Tick(720f, 0f, 0, 10);
            big.Tick(720f, 0f, 0, 10);
            float smallDrop = 0.21f - small.OxygenFraction;
            float bigDrop = 0.21f - big.OxygenFraction;
            Assert.That(bigDrop, Is.GreaterThan(0f));
            Assert.That(bigDrop, Is.EqualTo(smallDrop / 10f).Within(1e-6f));
        }

        [Test]
        public void Tick_ProductionOffsetsConsumptionAndIsCappedAtPureOxygen()
        {
            var air = Atmosphere();
            air.Tick(720f, 1f, 0, 10);
            Assert.That(air.OxygenFraction, Is.EqualTo(0.21f).Within(1e-6f));
            air.Tick(1f, 1000f, 0, 0);
            Assert.That(air.OxygenFraction, Is.EqualTo(1f).Within(1e-6f));
            Assert.That(air.Band, Is.EqualTo(AirBand.Green));
        }

        [Test]
        public void Tick_NeverGoesBelowZero()
        {
            var air = Atmosphere();
            air.Tick(720000f, 0f, 0, 100);
            Assert.That(air.OxygenKg, Is.Zero);
            Assert.That(air.Band, Is.EqualTo(AirBand.Critical));
        }

        [Test]
        public void AddModule_LowersFractionOnceAndAddsVolumeAndLeak()
        {
            var air = Atmosphere();
            var bands = new List<AirBand>();
            air.OnBandChanged += band => bands.Add(band);
            air.AddModule(50f, 0.72f);
            // (6,3 + 50 x 0,16 x 1,2) / (75 x 1,2) = 15,9 / 90.
            Assert.That(air.VolumeM3, Is.EqualTo(75f));
            Assert.That(air.OxygenFraction, Is.EqualTo(15.9f / 90f).Within(1e-5f));
            Assert.That(air.LeakKgPerSecond, Is.EqualTo(0.72f / 720f).Within(1e-8f));
            Assert.That(bands, Is.EqualTo(new[] { AirBand.Yellow }));
            float after = air.OxygenFraction;
            air.Tick(0f, 0f, 0, 0);
            Assert.That(air.OxygenFraction, Is.EqualTo(after));
        }

        [Test]
        public void AddModule_LeakKeepsDrainingWithoutCats()
        {
            var air = Atmosphere();
            air.AddModule(25f, 0.72f);
            float before = air.OxygenKg;
            air.Tick(720f, 0f, 0, 0);
            Assert.That(air.OxygenKg, Is.EqualTo(before - 0.72f).Within(1e-4f));
        }

        [Test]
        public void AddModule_LeaksAccumulateAcrossModules()
        {
            var air = Atmosphere();
            air.AddModule(10f, 0.36f);
            air.AddModule(10f, 0.72f);
            Assert.That(air.LeakKgPerSecond, Is.EqualTo(1.08f / 720f).Within(1e-8f));
            Assert.That(air.VolumeM3, Is.EqualTo(45f));
        }

        [Test]
        public void AddModule_RejectsInvalidArguments()
        {
            var air = Atmosphere();
            Assert.Throws<ArgumentOutOfRangeException>(() => air.AddModule(0f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => air.AddModule(-5f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => air.AddModule(10f, -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => air.AddModule(float.NaN, 0f));
            Assert.That(air.VolumeM3, Is.EqualTo(25f));
        }

        [Test]
        public void Tick_RejectsInvalidArguments()
        {
            var air = Atmosphere();
            Assert.Throws<ArgumentOutOfRangeException>(() => air.Tick(-1f, 0f, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => air.Tick(float.NaN, 0f, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => air.Tick(1f, -1f, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => air.Tick(1f, 0f, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => air.Tick(1f, 0f, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => air.Tick(1f, 0f, 0, 0, -1f));
        }

        [TestCase(0.21f, AirBand.Green)]
        [TestCase(0.196f, AirBand.Green)]
        [TestCase(0.194f, AirBand.Yellow)]
        [TestCase(0.161f, AirBand.Yellow)]
        [TestCase(0.159f, AirBand.Red)]
        [TestCase(0.101f, AirBand.Red)]
        [TestCase(0.099f, AirBand.Critical)]
        [TestCase(0.05f, AirBand.Critical)]
        public void Constructor_ClassifiesTheBandByFraction(float fraction, AirBand expected)
        {
            Assert.That(Atmosphere(fraction).Band, Is.EqualTo(expected));
        }

        [Test]
        public void Tick_ChangesBandAtEachLimitAndRaisesTheEventOncePerChange()
        {
            var air = Atmosphere();
            var bands = new List<AirBand>();
            air.OnBandChanged += band => bands.Add(band);
            // Un gato trabajando gasta 0,1 kg por sol: se baja de a poco hasta cruzar cada límite.
            for (int i = 0; i < 2000 && air.Band != AirBand.Critical; i++)
                air.Tick(10f, 0f, 0, 100);
            Assert.That(bands, Is.EqualTo(new[] { AirBand.Yellow, AirBand.Red, AirBand.Critical }));
            Assert.That(air.OxygenFraction, Is.LessThan(0.1f));
        }

        [Test]
        public void Tick_RecoversToHigherBands()
        {
            var air = Atmosphere(0.09f);
            var bands = new List<AirBand>();
            air.OnBandChanged += band => bands.Add(band);
            air.Tick(1f, 10f, 0, 0);
            air.Tick(1f, 1f, 0, 0);
            Assert.That(bands[bands.Count - 1], Is.EqualTo(air.Band));
            Assert.That(air.Band, Is.Not.EqualTo(AirBand.Critical));
        }

        [Test]
        public void CriticalTimeout_FiresOnlyAfterTheMarginAndOnlyOnce()
        {
            var air = Atmosphere(0.09f);
            Assert.That(air.Band, Is.EqualTo(AirBand.Critical));
            int timeouts = 0;
            air.OnCriticalTimeout += () => timeouts++;
            air.Tick(0f, 0f, 0, 0, 59f);
            Assert.That(timeouts, Is.Zero);
            air.Tick(0f, 0f, 0, 0, 1f);
            Assert.That(timeouts, Is.EqualTo(1));
            air.Tick(0f, 0f, 0, 0, 500f);
            Assert.That(timeouts, Is.EqualTo(1));
        }

        [Test]
        public void CriticalTimeout_CountsRealSecondsNotGameSeconds()
        {
            var air = Atmosphere(0.09f);
            int timeouts = 0;
            air.OnCriticalTimeout += () => timeouts++;
            air.Tick(180f, 0f, 0, 0, 20f);
            air.Tick(180f, 0f, 0, 0, 20f);
            Assert.That(timeouts, Is.Zero);
            air.Tick(180f, 0f, 0, 0, 20f);
            Assert.That(timeouts, Is.EqualTo(1));
        }

        [Test]
        public void CriticalTimeout_FourArgumentTickUsesGameDtAsRealDt()
        {
            var air = Atmosphere(0.09f);
            int timeouts = 0;
            air.OnCriticalTimeout += () => timeouts++;
            air.Tick(59f, 0f, 0, 0);
            Assert.That(timeouts, Is.Zero);
            air.Tick(1f, 0f, 0, 0);
            Assert.That(timeouts, Is.EqualTo(1));
        }

        [Test]
        public void CriticalTimeout_ResetsWhenTheAirRecovers()
        {
            var air = Atmosphere(0.09f);
            int timeouts = 0;
            air.OnCriticalTimeout += () => timeouts++;
            air.Tick(0f, 0f, 0, 0, 50f);
            air.Tick(1f, 20f, 0, 0, 1f);
            Assert.That(air.Band, Is.Not.EqualTo(AirBand.Critical));
            air.Tick(7200f, 0f, 0, 100, 0f);
            Assert.That(air.Band, Is.EqualTo(AirBand.Critical));
            air.Tick(0f, 0f, 0, 0, 59f);
            Assert.That(timeouts, Is.Zero);
            air.Tick(0f, 0f, 0, 0, 1f);
            Assert.That(timeouts, Is.EqualTo(1));
        }

        [Test]
        public void CriticalTimeout_DoesNotFireOutsideCritical()
        {
            var air = Atmosphere();
            int timeouts = 0;
            air.OnCriticalTimeout += () => timeouts++;
            air.Tick(0f, 0f, 0, 0, 1000f);
            Assert.That(timeouts, Is.Zero);
        }
    }
}
