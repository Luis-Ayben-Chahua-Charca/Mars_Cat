using System;
using MarsCat.Logic.Cats;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class CatTests
    {
        internal static CatConfig Config(float spread = 0f)
        {
            return new CatConfig(240f, 480f, 600f, 2f, 120f, 240f, 300f, 3f, spread);
        }

        [Test]
        public void TryAssign_OnlyFreeCatsCanWorkAndReleaseMakesThemFree()
        {
            var cat = new Cat(0, Config());
            Assert.That(cat.Health, Is.EqualTo(1f));
            Assert.That(cat.TryAssign(false), Is.True);
            Assert.That(cat.State, Is.EqualTo(CatState.WorkingIndoor));
            Assert.That(cat.TryAssign(true), Is.False);
            cat.Release();
            Assert.That(cat.State, Is.EqualTo(CatState.Free));
            Assert.That(cat.TryAssign(true), Is.True);
            Assert.That(cat.State, Is.EqualTo(CatState.WorkingOutdoor));
        }

        [Test]
        public void TryAssign_DegradedCatCanOnlyWorkIndoors()
        {
            var cat = new Cat(0, Config(), isDegraded: true);
            Assert.That(cat.TryAssign(true), Is.False);
            Assert.That(cat.IsDegraded, Is.True);
            Assert.That(cat.TryAssign(false), Is.True);
        }

        [Test]
        public void Constructor_RejectsInvalidIdentityConfigurationAndNeeds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Cat(-1, Config()));
            Assert.Throws<ArgumentNullException>(() => new Cat(0, null));
            foreach (float value in new[] { -1f, 1f, float.NaN, float.PositiveInfinity })
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new Cat(0, Config(), value));
                Assert.Throws<ArgumentOutOfRangeException>(() => new Cat(0, Config(), thirst: value));
            }
        }
    }
}
