using System;
using System.Collections.Generic;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Food;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class RatHuntTests
    {
        private ResourcePool _pool;
        private CatColony _colony;
        private LooseRats _loose;
        private RatHunt _hunt;

        private void Build(float food, int looseRats)
        {
            _pool = RatFarmTests.Pool(0f, food);
            // Agua de sobra para que la sed no mate al gato durante las pruebas.
            _pool.Add(ResourceId.Water, 1000f);
            _colony = new CatColony(CatTests.Config(), _pool, 0);
            _colony.Spawn(1);
            _loose = new LooseRats(looseRats);
            _hunt = new RatHunt(_colony, _loose, _pool, new RatHuntConfig(30f, 2f));
        }

        // Lleva al gato a hambre extrema (480 s) sin que haya raciones que lo alimenten.
        private void MakeStarving()
        {
            _colony.Tick(480f, false);
            Assert.That(_colony.Cats[0].State, Is.EqualTo(CatState.SeekingFood));
        }

        private void Step(float dt)
        {
            _colony.Tick(dt, false);
            _hunt.Tick(dt);
        }

        [Test]
        public void StarvingCatWithoutRationsHuntsALooseRat()
        {
            Build(0f, 3);
            var hunted = new List<int>();
            _hunt.OnHunted += (cat, id) => hunted.Add(id);
            MakeStarving();
            Assert.That(_colony.Cats[0].Hunger, Is.GreaterThan(0.79f));
            Step(29f);
            Assert.That(_loose.Count, Is.EqualTo(3));
            Step(1f);
            Assert.That(_loose.Count, Is.EqualTo(2));
            Assert.That(hunted, Is.EqualTo(new List<int> { 0 }));
            Assert.That(_colony.Cats[0].Hunger, Is.Zero);
        }

        [Test]
        public void CatWithRationsInPoolDoesNotHunt()
        {
            Build(0f, 3);
            MakeStarving();
            _pool.Add(ResourceId.Food, 2f);
            _hunt.Tick(60f);
            Assert.That(_loose.Count, Is.EqualTo(3));
        }

        [Test]
        public void HuntProgressResetsWhenRationsAppear()
        {
            Build(0f, 3);
            MakeStarving();
            _hunt.Tick(20f);
            _pool.Add(ResourceId.Food, 2f);
            _hunt.Tick(1f);
            _pool.TryConsume(ResourceId.Food, 2f);
            _hunt.Tick(20f);
            Assert.That(_loose.Count, Is.EqualTo(3));
            _hunt.Tick(10f);
            Assert.That(_loose.Count, Is.EqualTo(2));
        }

        [Test]
        public void FedCatThatIsNotSeekingFoodDoesNotHunt()
        {
            Build(0f, 3);
            _hunt.Tick(300f);
            Assert.That(_loose.Count, Is.EqualTo(3));
        }

        [Test]
        public void NoLooseRatsNotifiesOnlyOnce()
        {
            Build(0f, 0);
            int notices = 0;
            _hunt.OnNoLooseRats += () => notices++;
            MakeStarving();
            _hunt.Tick(30f);
            _hunt.Tick(30f);
            _hunt.Tick(30f);
            Assert.That(notices, Is.EqualTo(1));
            Assert.That(_colony.Cats[0].Hunger, Is.GreaterThan(0.79f));
        }

        [Test]
        public void LastRatIsHuntedThenColonyReportsNoneLeft()
        {
            Build(0f, 1);
            int notices = 0;
            _hunt.OnNoLooseRats += () => notices++;
            MakeStarving();
            Step(30f);
            Assert.That(_loose.Count, Is.Zero);
            Assert.That(notices, Is.Zero);
            Step(480f);
            Assert.That(_colony.Cats[0].State, Is.EqualTo(CatState.SeekingFood));
            _hunt.Tick(30f);
            Assert.That(notices, Is.EqualTo(1));
        }

        [Test]
        public void Tick_RejectsInvalidTimeAndConstructorNulls()
        {
            Build(0f, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => _hunt.Tick(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => _hunt.Tick(float.NaN));
            var config = new RatHuntConfig(30f, 2f);
            Assert.Throws<ArgumentNullException>(() => new RatHunt(null, _loose, _pool, config));
            Assert.Throws<ArgumentNullException>(() => new RatHunt(_colony, null, _pool, config));
            Assert.Throws<ArgumentNullException>(() => new RatHunt(_colony, _loose, null, config));
            Assert.Throws<ArgumentNullException>(() => new RatHunt(_colony, _loose, _pool, null));
        }
    }
}
