using System;
using System.Text;
using MarsCat.Logic.Energy;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class SolarCleaningSessionTests
    {
        private static ResourcePool Pool(float electricity)
        {
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.Electricity] = electricity;
            return new ResourcePool(new ResourceConfig(amounts, ResourceConfigTests.FiniteValues(1000f)));
        }

        private static SolarCleaningConfig Config(int rows = 4, float prob = 1f, float shot = 2f)
        {
            return new SolarCleaningConfig(3, 1f, rows, prob, shot);
        }

        [Test]
        public void Fire_AtCleanCellSpendsEnergyAndDoesNotScore()
        {
            var pool = Pool(10f);
            var session = new SolarCleaningSession(Config(prob: 0f), pool, 1);
            Assert.That(session.Fire(1), Is.True);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(8f));
            Assert.That(session.CleanedCells, Is.Zero);
        }

        [Test]
        public void Fire_AtDirtyCellCleansItAndChargesEveryShot()
        {
            var pool = Pool(10f);
            var session = new SolarCleaningSession(Config(), pool, 1);
            Assert.That(session.CurrentRow, Is.EqualTo(new[] { true, true, true }));
            session.Fire(0);
            Assert.That(session.CurrentRow, Is.EqualTo(new[] { false, true, true }));
            session.Fire(0);
            Assert.That(session.CleanedCells, Is.EqualTo(1));
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(6f));
        }

        [Test]
        public void Fire_WithoutElectricityDoesNotFire()
        {
            var pool = Pool(1f);
            var session = new SolarCleaningSession(Config(), pool, 1);
            Assert.That(session.Fire(0), Is.False);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(1f));
            Assert.That(session.CurrentRow[0], Is.True);
            Assert.That(session.CleanedCells, Is.Zero);
        }

        [Test]
        public void Fire_InvalidLaneThrows()
        {
            var session = new SolarCleaningSession(Config(), Pool(10f), 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Fire(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Fire(3));
        }

        [Test]
        public void Tick_UncleanedRowPassesAfterRowSecondsAndReportsMissed()
        {
            var session = new SolarCleaningSession(Config(), Pool(10f), 1);
            int cleaned = -1, missed = -1, passed = 0;
            session.OnRowPassed += (c, m) => { cleaned = c; missed = m; passed++; };
            session.Tick(0.99f);
            Assert.That(passed, Is.Zero);
            Assert.That(session.RowsRemaining, Is.EqualTo(4));
            session.Tick(0.01f);
            Assert.That(passed, Is.EqualTo(1));
            Assert.That(cleaned, Is.Zero);
            Assert.That(missed, Is.EqualTo(3));
            Assert.That(session.RowsRemaining, Is.EqualTo(3));
            Assert.That(session.Efficiency, Is.Zero);
        }

        [Test]
        public void Tick_ReportsCleanedCellsOfPassedRow()
        {
            var session = new SolarCleaningSession(Config(), Pool(10f), 1);
            int cleaned = -1, missed = -1;
            session.OnRowPassed += (c, m) => { cleaned = c; missed = m; };
            session.Fire(0);
            session.Fire(2);
            session.Tick(1f);
            Assert.That(cleaned, Is.EqualTo(2));
            Assert.That(missed, Is.EqualTo(1));
        }

        [Test]
        public void Tick_LargeDtPassesSeveralRowsAndFinishesOnce()
        {
            var session = new SolarCleaningSession(Config(), Pool(10f), 1);
            int passed = 0, finished = 0;
            PreparationResult result = default;
            session.OnRowPassed += (c, m) => passed++;
            session.OnFinished += r => { finished++; result = r; };
            session.Tick(100f);
            Assert.That(passed, Is.EqualTo(4));
            Assert.That(finished, Is.EqualTo(1));
            Assert.That(session.IsFinished, Is.True);
            Assert.That(session.RowsRemaining, Is.Zero);
            Assert.That(result.Starts, Is.True);
            Assert.That(result.Yield, Is.Zero);
            session.Tick(5f);
            Assert.That(finished, Is.EqualTo(1));
        }

        [Test]
        public void Efficiency_IsCleanedDirtyOverTotalDirty()
        {
            var session = new SolarCleaningSession(Config(rows: 2), Pool(100f), 1);
            Assert.That(session.TotalDirty, Is.EqualTo(6));
            session.Fire(0);
            session.Fire(1);
            session.Fire(2);
            session.Tick(1f);
            Assert.That(session.Efficiency, Is.EqualTo(0.5f).Within(1e-6f));
            session.Fire(1);
            session.Tick(1f);
            Assert.That(session.Efficiency, Is.EqualTo(4f / 6f).Within(1e-6f));
        }

        [Test]
        public void Finish_DeliversEfficiencyAsYield()
        {
            var session = new SolarCleaningSession(Config(rows: 1), Pool(100f), 1);
            PreparationResult result = default;
            session.OnFinished += r => result = r;
            session.Fire(0);
            session.Fire(1);
            session.Fire(2);
            session.Tick(1f);
            Assert.That(result.Yield, Is.EqualTo(1f));
            Assert.That(session.CurrentRow, Is.EqualTo(new[] { false, false, false }));
        }

        [Test]
        public void NoDirtyCellsInBand_EfficiencyIsOne()
        {
            var session = new SolarCleaningSession(Config(prob: 0f), Pool(10f), 1);
            Assert.That(session.TotalDirty, Is.Zero);
            Assert.That(session.Efficiency, Is.EqualTo(1f));
            PreparationResult result = default;
            session.OnFinished += r => result = r;
            session.Tick(100f);
            Assert.That(result.Starts, Is.True);
            Assert.That(result.Yield, Is.EqualTo(1f));
        }

        [Test]
        public void SameSeedRepeatsBandAndDifferentSeedChangesIt()
        {
            var cfg = new SolarCleaningConfig(3, 1f, 30, 0.4f, 0f);
            string a = Band(new SolarCleaningSession(cfg, Pool(0f), 42));
            string b = Band(new SolarCleaningSession(cfg, Pool(0f), 42));
            string c = Band(new SolarCleaningSession(cfg, Pool(0f), 43));
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
        }

        [Test]
        public void Fire_AfterFinishDoesNothingAndDoesNotCharge()
        {
            var pool = Pool(10f);
            var session = new SolarCleaningSession(Config(rows: 1), pool, 1);
            session.Tick(1f);
            Assert.That(session.Fire(0), Is.False);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(10f));
        }

        [Test]
        public void InvalidArgumentsThrow()
        {
            var session = new SolarCleaningSession(Config(), Pool(10f), 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(float.NaN));
            Assert.Throws<ArgumentNullException>(() => new SolarCleaningSession(null, Pool(1f), 1));
            Assert.Throws<ArgumentNullException>(() => new SolarCleaningSession(Config(), null, 1));
        }

        private static string Band(SolarCleaningSession session)
        {
            var sb = new StringBuilder();
            while (!session.IsFinished)
            {
                foreach (bool dirty in session.CurrentRow)
                    sb.Append(dirty ? '1' : '0');
                session.Tick(1f);
            }
            return sb.ToString();
        }
    }
}
