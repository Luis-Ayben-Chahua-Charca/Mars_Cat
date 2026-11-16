// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class MultiStepPreparationTests
    {
        // Paso de prueba con puntaje fijo, para comprobar que un tercer paso no toca a los otros dos.
        private sealed class FixedStep : IPreparationStep
        {
            public string Id { get; }
            public StepScore Score { get; set; }
            public int Charges { get; private set; }

            public FixedStep(string id, float score)
            {
                Id = id;
                Score = StepScore.Scored(score);
            }

            public StepScore Evaluate() { return Score; }
            public bool CanCharge() { return true; }
            public bool TryCharge() { Charges++; return true; }
        }

        private static ResourcePool Pool(float water, float compost)
        {
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.Water] = water;
            amounts[ResourceId.Compost] = compost;
            return new ResourcePool(new ResourceConfig(amounts, ResourceConfigTests.FiniteValues(100f)));
        }

        private static YieldConfig Yield(IDictionary<string, float> weights = null)
        {
            return new YieldConfig(0.8f, 1.2f, 0.1f, 1f, weights);
        }

        private static WashStep Wash(ResourcePool pool, float liters)
        {
            var step = new WashStep(new WashConfig(0.25f, 0.5f, 0.1f, 1f, 1f, 2f, 0.1f), pool);
            step.SetWater(liters);
            return step;
        }

        private static MixStep Mix(ResourcePool pool, float compost, float bacteria = 5f)
        {
            var step = new MixStep(new MixConfig(1f, 20f, 0f, 30f, 5f, 1f, 10f, 0.1f), pool);
            step.SetCompost(compost);
            step.SetBacteria(bacteria);
            return step;
        }

        private static MultiStepPreparation Soil(ResourcePool pool, float liters, float compost,
            IDictionary<string, float> weights = null)
        {
            return new MultiStepPreparation(
                new IPreparationStep[] { Wash(pool, liters), Mix(pool, compost) }, Yield(weights));
        }

        [Test]
        public void Confirm_AtTheGuideYieldsOnePointTwo()
        {
            var prep = Soil(Pool(5f, 5f), 1f, 20f);
            PreparationResult result = prep.Confirm();
            Assert.That(result.Starts, Is.True);
            Assert.That(result.Yield, Is.EqualTo(1.2f).Within(1e-5f));
            Assert.That(prep.ImproveKey, Is.Empty);
        }

        [Test]
        public void Confirm_TwentyPercentCompostAndOneAndAHalfGuideWaterYieldOnePointTen()
        {
            var prep = Soil(Pool(5f, 5f), 1.5f, 20f);
            Assert.That(prep.Score, Is.EqualTo(0.75f).Within(1e-5f));
            Assert.That(prep.Confirm().Yield, Is.EqualTo(1.10f).Within(1e-5f));
            Assert.That(prep.ImproveKey, Is.EqualTo("prep.wash.improve"));
        }

        [Test]
        public void Confirm_ZeroCompostStartsButYieldsLessAndAsksForMoreCompost()
        {
            var prep = Soil(Pool(5f, 5f), 1f, 0f);
            PreparationResult result = prep.Confirm();
            Assert.That(result.Starts, Is.True);
            Assert.That(result.Yield, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(prep.ImproveKey, Is.EqualTo("prep.mix.improve"));
        }

        [Test]
        public void Confirm_EverythingAtTheEdgeYieldsPointEight()
        {
            var prep = Soil(Pool(5f, 5f), 2f, 0f);
            Assert.That(prep.Confirm().Yield, Is.EqualTo(0.8f).Within(1e-5f));
        }

        [Test]
        public void Confirm_ChargesEveryStepWhenItStarts()
        {
            ResourcePool pool = Pool(5f, 5f);
            Soil(pool, 1.5f, 20f).Confirm();
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(3.5f).Within(1e-5f));
            Assert.That(pool.Get(ResourceId.Compost), Is.EqualTo(4.8f).Within(1e-5f));
        }

        [Test]
        public void Confirm_OutOfRangeStepDoesNotStartAndDoesNotCharge()
        {
            ResourcePool pool = Pool(5f, 5f);
            var prep = Soil(pool, 0.5f, 20f);
            PreparationResult result = prep.Confirm();
            Assert.That(result.Starts, Is.False);
            Assert.That(result.ReasonKey, Is.EqualTo("prep.wash.too_low"));
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(5f));
            Assert.That(pool.Get(ResourceId.Compost), Is.EqualTo(5f));
            Assert.That(prep.ImproveKey, Is.Empty);
            Assert.That(prep.Score, Is.Zero);
        }

        [Test]
        public void Confirm_BurningCompostNamesTheStepThatFailed()
        {
            ResourcePool pool = Pool(5f, 5f);
            PreparationResult result = Soil(pool, 1f, 31f).Confirm();
            Assert.That(result.Starts, Is.False);
            Assert.That(result.ReasonKey, Is.EqualTo("prep.mix.compost.too_high"));
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(5f));
        }

        [Test]
        public void Confirm_WithoutEnoughResourcesFailsWithoutChargingAnyStep()
        {
            ResourcePool pool = Pool(5f, 0.1f);
            PreparationResult result = Soil(pool, 1f, 20f).Confirm();
            Assert.That(result.Starts, Is.False);
            Assert.That(result.ReasonKey, Is.EqualTo("prep.mix.insufficient"));
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(5f));
            Assert.That(pool.Get(ResourceId.Compost), Is.EqualTo(0.1f));
        }

        [Test]
        public void Confirm_CanBeRepeatedAfterCorrectingTheInput()
        {
            ResourcePool pool = Pool(5f, 5f);
            var wash = Wash(pool, 0.5f);
            var prep = new MultiStepPreparation(new IPreparationStep[] { wash, Mix(pool, 20f) }, Yield());
            Assert.That(prep.Confirm().Starts, Is.False);
            wash.SetWater(1f);
            Assert.That(prep.Confirm().Starts, Is.True);
        }

        [Test]
        public void ThirdStep_WithZeroWeightDoesNotChangeTheOtherTwo()
        {
            ResourcePool pool = Pool(5f, 5f);
            var third = new FixedStep("light", 0f);
            var weights = new Dictionary<string, float> { { "light", 0f } };
            var prep = new MultiStepPreparation(
                new IPreparationStep[] { Wash(pool, 1.5f), Mix(pool, 20f), third }, Yield(weights));
            Assert.That(prep.Confirm().Yield, Is.EqualTo(1.10f).Within(1e-5f));
            Assert.That(third.Charges, Is.EqualTo(1));
        }

        [Test]
        public void ThirdStep_WithDefaultWeightJoinsTheAverageAndAsksToImproveItself()
        {
            ResourcePool pool = Pool(5f, 5f);
            var prep = new MultiStepPreparation(
                new IPreparationStep[] { Wash(pool, 1.5f), Mix(pool, 20f), new FixedStep("light", 0.25f) }, Yield());
            // (0,5 + 1 + 0,25) / 3
            Assert.That(prep.Score, Is.EqualTo(0.58333f).Within(1e-4f));
            Assert.That(prep.ImproveKey, Is.EqualTo("prep.light.improve"));
        }

        [Test]
        public void ThirdStep_OutOfRangeBlocksTheWholePreparation()
        {
            ResourcePool pool = Pool(5f, 5f);
            var third = new FixedStep("light", 1f) { Score = StepScore.OutOfRange("prep.light.too_high") };
            var prep = new MultiStepPreparation(
                new IPreparationStep[] { Wash(pool, 1f), Mix(pool, 20f), third }, Yield());
            PreparationResult result = prep.Confirm();
            Assert.That(result.Starts, Is.False);
            Assert.That(result.ReasonKey, Is.EqualTo("prep.light.too_high"));
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(5f));
        }

        [Test]
        public void Weights_ShiftTheAverageTowardTheHeavierStep()
        {
            var weights = new Dictionary<string, float> { { "wash", 3f }, { "mix", 1f } };
            // Agua 1,5 x guía (0,5) con peso 3 y abono en la guía (1) con peso 1: (1,5 + 1) / 4.
            var prep = Soil(Pool(5f, 5f), 1.5f, 20f, weights);
            Assert.That(prep.Score, Is.EqualTo(0.625f).Within(1e-5f));
            Assert.That(prep.Confirm().Yield, Is.EqualTo(0.8f + 0.4f * 0.625f).Within(1e-5f));
        }

        [Test]
        public void ImproveKey_PicksTheLowestScoringStep()
        {
            var prep = Soil(Pool(5f, 5f), 1.2f, 10f);
            Assert.That(prep.ImproveKey, Is.EqualTo("prep.mix.improve"));
        }

        [Test]
        public void Constructor_RejectsInvalidInput()
        {
            Assert.Throws<ArgumentNullException>(() => new MultiStepPreparation(null, Yield()));
            Assert.Throws<ArgumentNullException>(() => new MultiStepPreparation(new IPreparationStep[] { new FixedStep("a", 1f) }, null));
            Assert.Throws<ArgumentException>(() => new MultiStepPreparation(new IPreparationStep[0], Yield()));
            Assert.Throws<ArgumentException>(() => new MultiStepPreparation(new IPreparationStep[] { null }, Yield()));
            Assert.Throws<ArgumentException>(() => new MultiStepPreparation(
                new IPreparationStep[] { new FixedStep("a", 1f), new FixedStep("a", 1f) }, Yield()));
            var zero = new Dictionary<string, float> { { "a", 0f } };
            Assert.Throws<ArgumentException>(() => new MultiStepPreparation(
                new IPreparationStep[] { new FixedStep("a", 1f) }, Yield(zero)));
        }

        [Test]
        public void YieldFor_RejectsScoresOutsideZeroToOne()
        {
            var prep = Soil(Pool(1f, 1f), 1f, 20f);
            Assert.Throws<ArgumentOutOfRangeException>(() => prep.YieldFor(1.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => prep.YieldFor(-0.1f));
            Assert.That(prep.YieldFor(0f), Is.EqualTo(0.8f));
        }

        [Test]
        public void ResultIsAPreparationResultTheBuildingCanUseDirectly()
        {
            PreparationResult ok = Soil(Pool(5f, 5f), 1f, 20f).Confirm();
            PreparationResult fail = Soil(Pool(5f, 5f), 0f, 20f).Confirm();
            Assert.That(ok.Starts, Is.True);
            Assert.That(ok.ReasonKey, Is.Empty);
            Assert.That(fail.Yield, Is.Zero);
        }
    }
}
