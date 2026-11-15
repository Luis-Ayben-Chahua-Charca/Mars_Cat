using System;
using MarsCat.Logic.Cats;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class CatConfigTests
    {
        [Test]
        public void Constructor_PreservesBothNeedSchedulesAndRations()
        {
            var config = CatTests.Config(0.3f);
            Assert.That(config.FoodIntervalSeconds, Is.EqualTo(240f));
            Assert.That(config.FoodAlertSeconds, Is.EqualTo(480f));
            Assert.That(config.FoodDeathSeconds, Is.EqualTo(600f));
            Assert.That(config.FoodRation, Is.EqualTo(2f));
            Assert.That(config.WaterIntervalSeconds, Is.EqualTo(120f));
            Assert.That(config.WaterAlertSeconds, Is.EqualTo(240f));
            Assert.That(config.WaterDeathSeconds, Is.EqualTo(300f));
            Assert.That(config.WaterRation, Is.EqualTo(3f));
            Assert.That(config.InitialNeedSpread, Is.EqualTo(0.3f));
        }

        [Test]
        public void Constructor_RejectsInvalidTimesRationsAndSpread()
        {
            for (int index = 0; index < 8; index++)
            {
                foreach (float invalid in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
                {
                    float[] values = { 2f, 4f, 5f, 1f, 2f, 4f, 5f, 1f };
                    values[index] = invalid;
                    Assert.Throws<ArgumentOutOfRangeException>(() => new CatConfig(
                        values[0], values[1], values[2], values[3],
                        values[4], values[5], values[6], values[7], 0f));
                }
            }
            Assert.Throws<ArgumentException>(() => new CatConfig(4f, 4f, 5f, 1f, 2f, 4f, 5f, 1f, 0f));
            Assert.Throws<ArgumentException>(() => new CatConfig(2f, 4f, 5f, 1f, 2f, 5f, 5f, 1f, 0f));
            foreach (float invalid in new[] { -1f, 1f, float.NaN, float.PositiveInfinity })
                Assert.Throws<ArgumentOutOfRangeException>(() => CatTests.Config(invalid));
        }
    }
}
