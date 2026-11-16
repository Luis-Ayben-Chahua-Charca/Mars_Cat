using System;
using System.Collections.Generic;
using MarsCat.Logic.Food;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class LooseRatsTests
    {
        [Test]
        public void Constructor_CreatesIdsFromZero()
        {
            var loose = new LooseRats(3);
            Assert.That(loose.Count, Is.EqualTo(3));
            Assert.That(loose.IsLoose(0), Is.True);
            Assert.That(loose.IsLoose(2), Is.True);
            Assert.That(loose.IsLoose(3), Is.False);
            Assert.That(loose.IsLoose(-1), Is.False);
            Assert.That(loose.AnyLoose(), Is.EqualTo(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LooseRats(-1));
        }

        [Test]
        public void TryTake_RemovesOnceAndNotifies()
        {
            var loose = new LooseRats(3);
            var taken = new List<int>();
            loose.OnTaken += taken.Add;
            Assert.That(loose.TryTake(1), Is.True);
            Assert.That(loose.TryTake(1), Is.False);
            Assert.That(loose.TryTake(9), Is.False);
            Assert.That(loose.Count, Is.EqualTo(2));
            Assert.That(loose.IsLoose(1), Is.False);
            Assert.That(taken, Is.EqualTo(new List<int> { 1 }));
        }

        [Test]
        public void AnyLoose_ReturnsMinusOneWhenNoneLeft()
        {
            var loose = new LooseRats(2);
            loose.TryTake(0);
            Assert.That(loose.AnyLoose(), Is.EqualTo(1));
            loose.TryTake(1);
            Assert.That(loose.AnyLoose(), Is.EqualTo(-1));
            Assert.That(loose.Count, Is.Zero);
        }

        [Test]
        public void ZeroRats_StartsEmpty()
        {
            var loose = new LooseRats(0);
            Assert.That(loose.Count, Is.Zero);
            Assert.That(loose.AnyLoose(), Is.EqualTo(-1));
        }
    }
}
